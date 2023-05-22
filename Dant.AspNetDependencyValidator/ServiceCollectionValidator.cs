﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Razor;
#endif

namespace Dant.AspNetDependencyValidator
{
    internal class ServiceCollectionValidator
    {
        public bool IsValid => FailedValidations.All(x => x.Severity != Severity.Error);
        public HashSet<FailedValidation> FailedValidations { get; set; } = new HashSet<FailedValidation>();

        private readonly ServiceLifetime _controllerLifetime = ServiceLifetime.Transient;
        // Ignore the scope check for some Microsoft implementations, they are registered as Transient, but are used in Singleton services
        // TODO maybe just ignore all Microsoft.* services?
        private readonly IEnumerable<Type> _ignoredForScopeValidation = new List<Type>()
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

        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<ServiceDescriptor> _registeredServices;
        private readonly HashSet<ServiceDescriptor> _validatedServices = new HashSet<ServiceDescriptor>();

        public ServiceCollectionValidator(IServiceCollection serviceCollection)
        {
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _registeredServices = serviceCollection.ToList();
        }

        public void ValidateServiceCollection()
        {
            foreach (var service in _registeredServices)
            {
                ValidateServiceInternal(Enumerable.Empty<Type>(), service);
            }
        }

        public void ValidateControllers(Assembly assembly)
        {
            var controllers = assembly.GetTypes().Where(x => (typeof(ControllerBase)).IsAssignableFrom(x));

            foreach (var controller in controllers)
            {
                ValidateServiceInternal(new Type[] { controller }, new ServiceDescriptor(controller, controller, _controllerLifetime));
                ValidateEndpoints(controller);
            }
        }

        public void ValidatePages(Assembly assembly)
        {
            var basePageClass = Type.GetType("Microsoft.AspNetCore.Mvc.RazorPages.PageModel, Microsoft.AspNetCore.Mvc.RazorPages");
            var pages = assembly.GetTypes().Where(x => basePageClass.IsAssignableFrom(x));

            foreach (var page in pages)
            {
                ValidateServiceInternal(new Type[] { page }, new ServiceDescriptor(page, page, _controllerLifetime));
                ValidateEndpoints(page);
            }
        }

        private void ValidateServiceInternal(IEnumerable<Type> parents, ServiceDescriptor service)
        {
            if (!_validatedServices.Add(service))
                return;

            // TODO idk why but it can't be resolved
            if (service.ServiceType.ToString() == "Microsoft.AspNetCore.SignalR.Internal.HubDispatcher`1[THub]")
                return;

            if (service.ImplementationType is null)
            {
                // If we have an instance or a factory then we consider this successfully resolved - even though it might not be true in case of implementation factory
                // TODO deeper checks
                if (service.ImplementationInstance != null || service.ImplementationFactory != null)
                    return;

                FailedValidations.Add(new FailedValidation(Severity.Error, service.ServiceType, "Service is registered but does not have implementation of any kind."));
                return;
            }

            var constructors = service.ImplementationType.GetConstructors();

            // Get the constructor with the ActivatorUtilitiesConstructor attribute, which is used by the DI to find the correct constructor in case of multiple 
            // constructors. For some reason, some of the Microsoft implementations have multiple constructors, mostly extended with ILogger<> parameter. I'm not 
            // sure how DI knows which one to use, but I assume it tries the one by one, starting with the one with most arguments, until it succeeds resolving all elements. 
            // I just grab the first one and consider it good enough, but it might require better implementation in the future. 
            var constructor = constructors.SingleOrDefault(x =>
                    x?.GetCustomAttribute<ActivatorUtilitiesConstructorAttribute>() != null) ?? constructors.FirstOrDefault();

            if (constructor is null)
            {
                FailedValidations.Add(new FailedValidation(Severity.Warning, service.ServiceType,
                    $"Service implementation {service.ImplementationType.Name} does not have valid constructors."));
                return;
            }

            var parameters = constructor.GetParameters();

            foreach (var parameterInfo in parameters)
            {
                ValidateChildService(parents.Append(service.ServiceType), parameterInfo.ParameterType, service.Lifetime);
            }
        }

        private void ValidateChildService(IEnumerable<Type> parents, Type serviceType, ServiceLifetime parentLifetime,
            Type explicitImplementationType = null, ServiceLifetime? explicitServiceLifetime = null)
        {
            // This one is, of course, resolvable even though it does not exist in the serviceCollection list.
            if (serviceType == typeof(IServiceProvider) || serviceType == typeof(IServiceScopeFactory))
                return;

#if NET6_0_OR_GREATER
            if (serviceType == typeof(IServiceProviderIsService))
                return;
#endif

#if NETCOREAPP3_1_OR_GREATER
            // https://stackoverflow.com/questions/58118280/iactioncontextaccessor-is-null
            if (serviceType == typeof(IActionContextAccessor))
                return;
#endif

            var matches = _registeredServices.Where(
                x => x.ServiceType == serviceType || (serviceType.IsGenericType && x.ServiceType == serviceType.GetGenericTypeDefinition())).ToList();

            if (!matches.Any())
            {
                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // IEnumerable<> dependencies are valid to resolve to empty enumerable
                    // TODO maybe warn?
                    return;
                }

                FailedValidations.Add(new FailedValidation(Severity.Error, serviceType, $"Failed to resolve {serviceType} needed to create {string.Join(" <- ", parents.Reverse())}"));

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
                ValidateServiceLifetime(match.ServiceType, parents.Last(), parentLifetime, match.Lifetime);
                ValidateServiceInternal(parents.Append(match.ServiceType), match);

                if (explicitServiceLifetime != null && match.Lifetime != explicitServiceLifetime)
                {
                    FailedValidations.Add(new FailedValidation(Severity.Error, serviceType,
                        $"Service is implemented with {match.Lifetime:G} service lifetime, but is required to have {explicitServiceLifetime:G} service lifetime."));
                }
            }
        }

        private void ValidateServiceLifetime(Type serviceType, Type parentType, ServiceLifetime parentLifetime, ServiceLifetime child)
        {
            // Never inject Scoped & Transient services into Singleton service.
            // Never inject Transient services into scoped service
            // Ignore certain microsoft implementations.
            if (_ignoredForScopeValidation.Contains(serviceType))
                return;

            switch (parentLifetime)
            {
                case ServiceLifetime.Singleton when child is ServiceLifetime.Scoped || child is ServiceLifetime.Transient:
                case ServiceLifetime.Scoped when child is ServiceLifetime.Transient:
                    FailedValidations.Add(new FailedValidation(Severity.Warning, serviceType,
                        $"Service {serviceType} is implemented with {child} service lifetime, but the parent ({parentType}) has {parentLifetime} service lifetime."));
                    break;
            }
        }

        private void ValidateEndpoints(Type controller)
        {
            var endpointMethods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any());

            var fromServiceParameters = endpointMethods.SelectMany(x => x.GetParameters())
                .Where(x => x.GetCustomAttributes<FromServicesAttribute>().Any());

            foreach (var parameterInfo in fromServiceParameters)
            {
                ValidateChildService(new Type[] { controller }, parameterInfo.ParameterType, _controllerLifetime);
            }
        }
    }
}
