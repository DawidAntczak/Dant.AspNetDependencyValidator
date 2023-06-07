using System;
using Dant.AspNetDependencyValidator.Validation;
using Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage;

namespace Dant.AspNetDependencyValidator.Builder
{
    public interface IAddAssembliesBuildStage : IAddValidationsBuildStage
    {
        IAddValidationsBuildStage WithAdditional(Func<IAssemblyCollectionBuilder, IAssemblyCollectionBuilder> assemblies);
    }

    public interface IAddValidationsBuildStage
    {
        IAddAssumedExistingTypesStage WithValidation(Func<IValidationCollectionBuilder, IValidationCollectionBuilder> including);
    }

    public interface IAddAssumedExistingTypesStage : IFinishStage
    {
        IFinishStage AssumingExistenceOf(Func<IAssumedServiceCollectionBuilder, IAssumedServiceCollectionBuilder> services);
    }

    public interface IFinishStage
    {
        IValidationRunner Build();
    }
}
