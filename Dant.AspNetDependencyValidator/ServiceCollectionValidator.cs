﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Razor;
#endif

namespace Dant.AspNetDependencyValidator
{
    internal class ServiceCollectionValidator
    {
        public bool IsValid => FailedValidations.All(x => x.Severity != Severity.Error);
        public HashSet<FailedValidation> FailedValidations { get; set; } = new HashSet<FailedValidation>();

        // Ignore the scope check for some Microsoft implementations, they are registered as Transient, but are used in Singleton services
        private readonly IEnumerable<Type> _ignoreForScopeValidation = new List<Type>()
        {
            typeof(IOptionsFactory<>),
            typeof(ICompositeMetadataDetailsProvider),
            typeof(IControllerActivator),
            typeof(IAuthorizationPolicyProvider),
            #if NETCOREAPP3_1_OR_GREATER
            typeof(IViewComponentDescriptorProvider),
            typeof(IRazorPageFactoryProvider)
            #endif
        };

        // Controllers are created as transient services
        private readonly ServiceLifetime _controllerLifetime = ServiceLifetime.Transient;

        private readonly IList<ServiceDescriptor> _services;
        private readonly HashSet<ServiceDescriptor> _validatedServices = new HashSet<ServiceDescriptor>();

        public ServiceCollectionValidator(IServiceCollection serviceCollection)
        {
            _services = serviceCollection.ToList();
        }

        public void ValidateServiceCollection()
        {
            foreach (var service in _services)
            {
                ValidateServiceInternal(service);
            }
        }

        public void ValidateControllers(Assembly assembly)
        {
            var baseControllers = assembly.GetTypes().Where(x => (typeof(ControllerBase)).IsAssignableFrom(x));

            foreach (var baseController in baseControllers)
            {
                ValidateController(baseController);

                ValidateControllerEndpoints(baseController);
            }
        }

        private void ValidateServiceInternal(ServiceDescriptor service)
        {
            if (_validatedServices.Contains(service))
                return;
            _validatedServices.Add(service);

            var serviceType = service.ServiceType;
            var implementation = service.ImplementationType;
            var lifetime = service.Lifetime;

            if (implementation is null)
            {
                // If we have an instance or a factory then we consider this successfully resolved - even though it might not be true in case of implementation factory
                if (service.ImplementationInstance != null || service.ImplementationFactory != null)
                    return;

                FailedValidations.Add(new FailedValidation(Severity.Error, serviceType, "Service is registered but does not have implementation of any kind."));
                return;
            }

            var constructors = implementation.GetConstructors();

            // Get the constructor with the ActivatorUtilitiesConstructor attribute, which is used by the DI to find the correct constructor in case of multiple 
            // constructors. For some reason, some of the Microsoft implementations have multiple constructors, mostly extended with ILogger<> parameter. I'm not 
            // sure how DI knows which one to use, but I assume it tries the one by one, starting with the one with most arguments, until it succeeds resolving all 
            // elements. 
            // I just grab the first one and consider it good enough, but it might require better implementation in the future. 
            var constructor = constructors.SingleOrDefault(x =>
                    x?.GetCustomAttribute<ActivatorUtilitiesConstructorAttribute>() != null) ?? constructors.FirstOrDefault();

            if (constructor is null)
            {
                FailedValidations.Add(new FailedValidation(Severity.Warning, serviceType,
                    $"Service implementation {implementation.Name} does not have valid constructors."));
                return;
            }

            var parameters = constructor.GetParameters();

            foreach (var parameterInfo in parameters)
            {
                ValidateChildService(parameterInfo.ParameterType, lifetime);
            }
        }

        private void ValidateChildService(Type serviceType, ServiceLifetime parentLifetime, Type explicitImplementationType = null,
            ServiceLifetime? explicitServiceLifetime = null)
        {
            // This one is, of course, resolvable even though it does not exist in the serviceCollection list.
            if (serviceType == typeof(IServiceProvider))
                return;

            var matches = _services.Where(
                x => x.ServiceType == serviceType || (serviceType.IsGenericType && x.ServiceType == serviceType.GetGenericTypeDefinition())).ToList();

            if (!matches.Any())
            {
                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // IEnumerable<> dependencies are valid to resolve to empty enumerable
                    return;
                }

                FailedValidations.Add(new FailedValidation(Severity.Error, serviceType, $"Failed to resolve {serviceType}."));
                return;
            }

            if (explicitImplementationType != null)
            {
                var explicitMatch = matches.SingleOrDefault(x => x.ImplementationType == explicitImplementationType);
                if (explicitMatch is null)
                {
                    FailedValidations.Add(new FailedValidation(Severity.Error, serviceType,
                        $"{serviceType} does not match required implementation type {explicitImplementationType}"));
                }
            }

            foreach (var match in matches)
            {
                ValidateServiceLifetime(match.ServiceType, parentLifetime, match.Lifetime);
                ValidateServiceInternal(match);

                if (explicitServiceLifetime != null && match.Lifetime != explicitServiceLifetime)
                {
                    FailedValidations.Add(new FailedValidation(Severity.Error, serviceType,
                        $"Service is implemented with {match.Lifetime:G} service lifetime, but is required to have {explicitServiceLifetime:G} service lifetime."));
                }
            }
        }

        private void ValidateServiceLifetime(Type serviceType, ServiceLifetime parent, ServiceLifetime child)
        {
            // Never inject Scoped & Transient services into Singleton service.
            // Never inject Transient services into scoped service
            // Ignore certain microsoft implementations.
            if (_ignoreForScopeValidation.Contains(serviceType))
                return;

            switch (parent)
            {
                case ServiceLifetime.Singleton when child is ServiceLifetime.Scoped || child is ServiceLifetime.Transient:
                case ServiceLifetime.Scoped when child is ServiceLifetime.Transient:
                    FailedValidations.Add(new FailedValidation(Severity.Warning, serviceType,
                        $"Service {serviceType} is implemented with {child:G} service lifetime, but the parent has {parent:G} service lifetime."));
                    break;
            }
        }

        private void ValidateController(Type controller)
        {
            // We can reuse ValidateServiceInternal nicely
            ValidateServiceInternal(new ServiceDescriptor(controller, controller, _controllerLifetime));
        }

        private void ValidateControllerEndpoints(Type controller)
        {
            var endpointMethods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any());

            foreach (var endpointMethod in endpointMethods)
            {
                var serviceParameters = endpointMethod.GetParameters()
                    .Where(x => x.GetCustomAttributes<FromServicesAttribute>().Any());

                foreach (var parameterInfo in serviceParameters)
                {
                    ValidateChildService(parameterInfo.ParameterType, _controllerLifetime);
                }
            }
        }
    }
}


