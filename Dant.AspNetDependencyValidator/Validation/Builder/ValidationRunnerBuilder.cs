using System.Collections.Generic;
using System.Reflection;
using System;
using Dant.AspNetDependencyValidator.Validation;
using Dant.AspNetDependencyValidator.Validation.ValidationLogic;
using Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage;

namespace Dant.AspNetDependencyValidator.Builder
{
    internal sealed class ValidationRunnerBuilder<TEntryPoint>
        : IAddAssembliesBuildStage, IAddValidationsBuildStage, IAddAssumedExistingServicesStage, IFinishStage
        where TEntryPoint : class
    {
        private readonly List<Assembly> _assemblies = new() { typeof(TEntryPoint).Assembly };
        private readonly List<Action<Validator>> _validations = new();
        private bool _onBuildValidation = false;
        private readonly HashSet<Type> _assumedExistingTypes = new();

        internal ValidationRunnerBuilder() { }

        public IAddValidationsBuildStage WithAdditional(Func<IAssemblyCollectionBuilder, IAssemblyCollectionBuilder> assemblies)
        {
            var builder = new AssemblyCollectionBuilder();
            assemblies(builder);
            _assemblies.AddRange(builder.Assemblies);
            return this;
        }

        public IAddAssumedExistingServicesStage WithValidation(Func<IValidationCollectionBuilder, IValidationCollectionBuilder> validations)
        {
            var builder = new ValidationCollectionBuilder(typeof(TEntryPoint).Assembly, _assemblies);
            validations(builder);
            _validations.AddRange(builder.Validations);
            _onBuildValidation = builder.OnBuildValidation;
            return this;
        }

        public IFinishStage AssumingExistenceOf(Func<IAssumedServiceCollectionBuilder, IAssumedServiceCollectionBuilder> services)
        {
            var builder = new AssumedServiceCollectionBuilder();
            services(builder);
            _assumedExistingTypes.UnionWith(builder.AssumedExistingServices);
            return this;
        }

        public IValidationRunner Build()
        {
            return new ValidationRunner<TEntryPoint>(_validations, _onBuildValidation, _assumedExistingTypes);
        }
    }
}
