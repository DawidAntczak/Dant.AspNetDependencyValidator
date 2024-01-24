using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceCollectionDIValidator.Validation.Result;
using Microsoft.AspNetCore.Http;

namespace ServiceCollectionDIValidator.Validation.ValidationLogic
{
    internal sealed class Validator
    {
        public ISet<FailedValidation> FailedValidations { get; } = new HashSet<FailedValidation>();

        private readonly ServiceLifetime _controllerLifetime = ServiceLifetime.Transient;
        private readonly IEnumerable<ServiceDescriptor> _registeredServices;
        private readonly HashSet<ServiceDescriptor> _validatedServices = new();

        private readonly HashSet<Type> _assumedExistingTypes = new()
        {
            typeof(IServiceProvider),
            typeof(IServiceScopeFactory),
            typeof(ILogger),
            typeof(RequestDelegate),    // injected into middlewares
            Type.GetType("Blazored.LocalStorage.ILocalStorageService, Blazored.LocalStorage")
        };

        public Validator(IServiceCollection serviceCollection, IEnumerable<Type> assumedExistingTypes)
        {
            _registeredServices = serviceCollection.ToList();
            foreach (var type in assumedExistingTypes)
            {
                _assumedExistingTypes.Add(type);
            }
        }

        public void ValidateEntryPoints(IEnumerable<Assembly> assemblies, Type baseTypeOfEntryPoint)
        {
            var entryPoints = assemblies.SelectMany(a => a.GetTypes()).Where(x => baseTypeOfEntryPoint.IsAssignableFrom(x));

            foreach (var entryPoint in entryPoints)
            {
                ValidateServiceInternal(new Type[] { }, new ServiceDescriptor(entryPoint, entryPoint, _controllerLifetime));
                ValidateEndpoints(entryPoint);
            }
        }

        public void ValidateEntryPoints(IEnumerable<Assembly> assemblies, IEnumerable<Type> exactTypesOfEntryPoints)
        {
            var entryPoints = assemblies.SelectMany(a => a.GetTypes()).Where(x => exactTypesOfEntryPoints.Contains(x));

            foreach (var entryPoint in entryPoints)
            {
                ValidateServiceInternal(new Type[] { }, new ServiceDescriptor(entryPoint, entryPoint, _controllerLifetime));
                ValidateEndpoints(entryPoint);
            }
        }

        public void ValidatePropertiesInjection(IEnumerable<Assembly> assemblies, Type baseTypeOfEntryPoint, Type injectAttributeType)
        {
            var entryPoints = assemblies.SelectMany(a => a.GetTypes()).Where(x => baseTypeOfEntryPoint.IsAssignableFrom(x));

            foreach (var entryPoint in entryPoints)
            {
                ValidatePropertiesInjection(entryPoint, injectAttributeType);
            }
        }

        public void ValidateServices(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                ValidateChildService(new List<Type>(), type, ServiceLifetime.Transient);
            }
        }

        private void ValidateServiceInternal(IEnumerable<Type> parents, ServiceDescriptor service)
        {
            if (!_validatedServices.Add(service))
                return;

            if (service.ImplementationType is null)
            {
                // If we have an instance or a factory then we consider this successfully resolved - even though it might not be true in case of implementation factory
                // TODO deeper checks
                if (service.ImplementationInstance != null || service.ImplementationFactory != null)
                    return;

                FailedValidations.Add(new FailedValidation(IssueType.MissingService, service.ServiceType, "Service is registered but does not have implementation of any kind."));
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
                FailedValidations.Add(new FailedValidation(IssueType.MissingService, service.ServiceType,
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
            if (_assumedExistingTypes.Contains(serviceType))
                return;

            var matches = _registeredServices.Where(
                x => x.ServiceType == serviceType || (serviceType.IsGenericType && x.ServiceType == serviceType.GetGenericTypeDefinition()))
                .ToList();

            if (!matches.Any())
            {
                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // IEnumerable<> dependencies are valid to resolve to empty enumerable
                    // TODO maybe warn?
                    return;
                }

                FailedValidations.Add(new FailedValidation(IssueType.MissingService, serviceType,
                    $"Failed to resolve {serviceType}" + (parents.Any() ? $" needed to create {string.Join(" <- ", parents.Reverse())}" : "")));

                return;
            }

            if (explicitImplementationType != null)
            {
                var explicitMatch = matches.SingleOrDefault(x => x.ImplementationType == explicitImplementationType);
                if (explicitMatch is null)
                {
                    FailedValidations.Add(new FailedValidation(IssueType.MissingService, serviceType,
                        $"{serviceType} does not match required implementation type {explicitImplementationType}"));
                }
            }

            foreach (var match in matches)
            {
                var parent = parents.LastOrDefault();
                if (parent != null)
                {
                    ValidateServiceLifetime(match.ServiceType, parent, parentLifetime, match.Lifetime);
                }
                ValidateServiceInternal(parents.Append(match.ServiceType), match);

                if (explicitServiceLifetime != null && match.Lifetime != explicitServiceLifetime)
                {
                    FailedValidations.Add(new FailedValidation(IssueType.MissingService, serviceType,
                        $"Service is implemented with {match.Lifetime} service lifetime, but is required to have {explicitServiceLifetime} service lifetime."));
                }
            }
        }

        private void ValidateServiceLifetime(Type serviceType, Type parentType, ServiceLifetime parentLifetime, ServiceLifetime child)
        {
            switch (parentLifetime)
            {
                case ServiceLifetime.Singleton when child is ServiceLifetime.Scoped || child is ServiceLifetime.Transient:
                case ServiceLifetime.Scoped when child is ServiceLifetime.Transient:
                    FailedValidations.Add(new FailedValidation(IssueType.IncosistentLifetime, serviceType,
                        $"Service {serviceType} is implemented with {child} service lifetime, but the parent ({parentType}) has {parentLifetime} service lifetime."));
                    break;
            }
        }

        private void ValidateEndpoints(Type endpointType)
        {
            var endpointMethods = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any());

            var fromServiceParameters = endpointMethods.SelectMany(x => x.GetParameters())
                .Where(x => x.GetCustomAttributes<FromServicesAttribute>().Any());

            foreach (var parameterInfo in fromServiceParameters)
            {
                ValidateChildService(new Type[] { endpointType }, parameterInfo.ParameterType, _controllerLifetime);
            }
        }

        private void ValidatePropertiesInjection(Type entryPoint, Type injectAttributeType)
        {
            var injectedProperties = entryPoint.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttributes(injectAttributeType, true).Any());

            foreach (var property in injectedProperties)
            {
                ValidateChildService(new Type[] { entryPoint }, property.PropertyType, _controllerLifetime);
            }
        }
    }
}
