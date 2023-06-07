using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Dant.AspNetDependencyValidator.Validation.ValidationLogic;
using Dant.AspNetDependencyValidator.CodeAnalysis.UsageFinder;

namespace Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage
{
    public interface IValidationCollectionBuilder
    {
        IValidationCollectionBuilder Controllers();
        IValidationCollectionBuilder Pages();
        IValidationCollectionBuilder GetRequiredServiceCalls();
        IValidationCollectionBuilder GetServiceCalls();
        IValidationCollectionBuilder CollectionBuild();
    }

    internal sealed class ValidationCollectionBuilder : IValidationCollectionBuilder
    {
        public List<Action<Validator>> Validations { get; } = new List<Action<Validator>>();
        public bool OnBuildValidation { get; private set; } = false;

        private readonly Assembly _entryPoint;
        private readonly IEnumerable<Assembly> _assembliesToValidate;

        public ValidationCollectionBuilder(Assembly entryPoint, IEnumerable<Assembly> assembliesToValidate)
        {
            _entryPoint = entryPoint;
            _assembliesToValidate = assembliesToValidate;
        }

        public IValidationCollectionBuilder Controllers()
        {
            Validations.Add(v => v.ValidateControllers(_entryPoint));
            return this;
        }

        public IValidationCollectionBuilder Pages()
        {
            Validations.Add(v => v.ValidatePages(_entryPoint));
            return this;
        }

        public IValidationCollectionBuilder GetRequiredServiceCalls()
        {
            return AddServiceProviderServiceExtensionsCalledTypesValidation("GetRequiredService");
        }

        public IValidationCollectionBuilder GetServiceCalls()
        {
            return AddServiceProviderServiceExtensionsCalledTypesValidation("GetService");
        }

        public IValidationCollectionBuilder CollectionBuild()
        {
            OnBuildValidation = true;
            return this;
        }

        /* TODO: Implement
        public ValidationBuilder<TEntryPoint> ValidateLifetime()
        {
            return this;
        }*/

        private IValidationCollectionBuilder AddServiceProviderServiceExtensionsCalledTypesValidation(string methodName)
        {
            var method = typeof(ServiceProviderServiceExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.ContainsGenericParameters && m.Name == methodName);

            var usages = _assembliesToValidate
                .Select(a => new GenericTypesUsageFinder(a.Location))
                .SelectMany(f => f.FindUsedByMethodGenericTypes(method));

            var requiredTypes = usages
                .Select(u => u.UsedType)
                .Distinct();

            Validations.Add(v => v.ValidateServices(requiredTypes));
            return this;
        }
    }
}
