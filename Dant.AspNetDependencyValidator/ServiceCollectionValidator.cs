using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dant.AspNetDependencyValidator.CallsFinding;
using Dant.AspNetDependencyValidator.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Dant.AspNetDependencyValidator
{
    public sealed class ServiceCollectionValidator<TEntryPoint> : IAddAssembliesBuildStage, IAddValidationsBuildStage, IAddAssumedExistingTypesStage
        where TEntryPoint : class
    {
        private readonly List<Assembly> _assemblies = new List<Assembly> { typeof(TEntryPoint).Assembly };
        private readonly List<Action<ServiceCollectionValidator>> _validations = new List<Action<ServiceCollectionValidator>>();
        private bool _onBuildValidation = false;
        private readonly HashSet<Type> _assumedExistingTypes = new HashSet<Type>();

        private ServiceCollectionValidator() { }

        public static IAddAssembliesBuildStage ForEntryAssembly()
        {
            return new ServiceCollectionValidator<TEntryPoint>();
        }

        public IAddAssembliesBuildStage And(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        public IAddAssembliesBuildStage And<TFromAssembly>()
        {
            _assemblies.Add(typeof(TFromAssembly).Assembly);
            return this;
        }

        public IAddValidationsBuildStage WithValidationsOf()
        {
            return this;
        }

        public IAddValidationsBuildStage Controllers()
        {
            _validations.Add(v => v.ValidateControllers(typeof(TEntryPoint).Assembly));
            return this;
        }

        public IAddValidationsBuildStage Pages()
        {
            _validations.Add(v => v.ValidatePages(typeof(TEntryPoint).Assembly));
            return this;
        }

        public IAddValidationsBuildStage OnBuild()
        {
            _onBuildValidation = true;
            return this;
        }

        public IAddValidationsBuildStage GetRequiredServiceTypes()
        {
            return AddServiceProviderServiceExtensionsCalledTypesValidation("GetRequiredService");
        }

        public IAddValidationsBuildStage GetServiceTypes()
        {
            return AddServiceProviderServiceExtensionsCalledTypesValidation("GetService");
        }

        /* TODO: Implement
        public ValidationBuilder<TEntryPoint> ValidateLifetime()
        {
            return this;
        }*/

        public IAddAssumedExistingTypesStage AssumingExistenceOf()
        {
            return this;
        }

        public IAddAssumedExistingTypesStage Service<T>()
        {
            _assumedExistingTypes.Add(typeof(T));
            return this;
        }

        public IAddAssumedExistingTypesStage Service(Type type)
        {
            _assumedExistingTypes.Add(type);
            return this;
        }

        public IValidator Build()
        {
            return new Validator<TEntryPoint>(_validations, _onBuildValidation, _assumedExistingTypes);
        }

        private IAddValidationsBuildStage AddServiceProviderServiceExtensionsCalledTypesValidation(string methodName)
        {
            var method = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.ContainsGenericParameters && m.Name == methodName);

            var usages = _assemblies
                .Select(a => new GenericTypesUsageFinder(a.Location))
                .SelectMany(f => f.FindUsedByMethodGenericTypes(method));

            var requiredTypes = usages
                .Select(u => u.UsedType)
                .Distinct();

            _validations.Add(v => v.ValidateServices(requiredTypes));
            return this;
        }
    }

    public interface IAddAssembliesBuildStage
    {
        IAddAssembliesBuildStage And(Assembly assembly);
        IAddAssembliesBuildStage And<TFromAssembly>();
        IAddValidationsBuildStage WithValidationsOf();
    }

    public interface IAddValidationsBuildStage
    {
        IAddValidationsBuildStage Controllers();
        IAddValidationsBuildStage Pages();
        IAddValidationsBuildStage GetRequiredServiceTypes();
        IAddValidationsBuildStage GetServiceTypes();
        IAddAssumedExistingTypesStage AssumingExistenceOf();
        // IAddValidationsBuildStage ValidateLifetime();
        IValidator Build();
    }

    public interface IAddAssumedExistingTypesStage
    {
        IAddAssumedExistingTypesStage Service<T>();
        IAddAssumedExistingTypesStage Service(Type type);
        IValidator Build();
    }
}

