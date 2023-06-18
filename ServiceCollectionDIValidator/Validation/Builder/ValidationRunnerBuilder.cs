﻿using System.Collections.Generic;
using System.Reflection;
using System;
using ServiceCollectionDIValidator.Validation;
using ServiceCollectionDIValidator.Validation.ValidationLogic;
using ServiceCollectionDIValidator.Validation.Builder.AddAssembliesStage;

namespace ServiceCollectionDIValidator.Builder
{
    internal sealed class ValidationRunnerBuilder<TEntryPoint>
        : IAddAssembliesBuildStage, IAddValidationsBuildStage, IAddAssumedExistingServicesStage, IFinishStage
        where TEntryPoint : class
    {
        private readonly List<Assembly> _assemblies = new() { typeof(TEntryPoint).Assembly };
        private readonly List<Action<Validator>> _validations = new();
        private readonly HashSet<Type> _assumedExistingTypes = new();

        internal ValidationRunnerBuilder() { }

        public IAddValidationsBuildStage WithAdditional(Action<IAssemblyCollectionBuilder> assemblies)
        {
            var builder = new AssemblyCollectionBuilder();
            assemblies(builder);
            _assemblies.AddRange(builder.Assemblies);
            return this;
        }

        public IAddAssumedExistingServicesStage WithValidation(Action<IValidationCollectionBuilder> validations)
        {
            var builder = new ValidationCollectionBuilder(_assemblies);
            validations(builder);
            _validations.AddRange(builder.Validations);
            return this;
        }

        public IFinishStage AssumingExistenceOf(Action<IAssumedServiceCollectionBuilder> services)
        {
            var builder = new AssumedServiceCollectionBuilder();
            services(builder);
            _assumedExistingTypes.UnionWith(builder.AssumedExistingServices);
            return this;
        }

        public IValidationRunner Build()
        {
            return new ValidationRunner<TEntryPoint>(_validations, _assumedExistingTypes);
        }
    }
}
