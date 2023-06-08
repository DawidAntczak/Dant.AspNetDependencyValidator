using System;
using Dant.AspNetDependencyValidator.Validation;
using Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage;

namespace Dant.AspNetDependencyValidator.Builder
{
    public interface IAddAssembliesBuildStage : IAddValidationsBuildStage
    {
        IAddValidationsBuildStage WithAdditional(Action<IAssemblyCollectionBuilder> assemblies);
    }

    public interface IAddValidationsBuildStage
    {
        IAddAssumedExistingServicesStage WithValidation(Action<IValidationCollectionBuilder> including);
    }

    public interface IAddAssumedExistingServicesStage : IFinishStage
    {
        IFinishStage AssumingExistenceOf(Action<IAssumedServiceCollectionBuilder> services);
    }

    public interface IFinishStage
    {
        IValidationRunner Build();
    }
}
