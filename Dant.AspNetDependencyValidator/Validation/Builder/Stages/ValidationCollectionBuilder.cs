using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Dant.AspNetDependencyValidator.Validation.ValidationLogic;
using Dant.AspNetDependencyValidator.CodeAnalysis.UsageFinder;
using Dant.AspNetDependencyValidator.Extensions;
using Dant.AspNetDependencyValidator.Validation.Builder.Stages;

namespace Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage
{
    public interface IValidationCollectionBuilder
    {
        IValidationCollectionBuilder Controllers();
        IValidationCollectionBuilder Pages();
        ITypesPassedMethodStage TypesPassed();
        IValidationCollectionBuilder TypesPassedToGetRequiredService();
        IValidationCollectionBuilder TypesPassedToGetService();
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

        public ITypesPassedMethodStage TypesPassed()
        {
            var onFinish = (TypesPassedBuilder builder) =>
            {
                using var usageFinders = _assembliesToValidate
                .Select(a => new GenericTypesUsageFinder(a.Location))
                .ToDisposableArray();

                var usages = usageFinders
                    .SelectMany(f => f.FindUsedByMethodGenericTypes(builder.MethodWithGenericParameters, builder.ParameterPosition));

                var requiredTypes = usages
                    .Select(u => u.UsedType)
                    .Distinct()
                    .ToArray();

                Validations.Add(v => v.ValidateServices(requiredTypes));
            };
            return new TypesPassedBuilder(this, onFinish);
        }

        public IValidationCollectionBuilder TypesPassedToGetRequiredService()
        {
            var method = typeof(ServiceProviderServiceExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.ContainsGenericParameters && m.Name == "GetRequiredService");
            return TypesPassed().To(method).AtPosition(0);
        }

        public IValidationCollectionBuilder TypesPassedToGetService()
        {
            var method = typeof(ServiceProviderServiceExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.ContainsGenericParameters && m.Name == "GetService");
            return TypesPassed().To(method).AtPosition(0);
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
    }
}
