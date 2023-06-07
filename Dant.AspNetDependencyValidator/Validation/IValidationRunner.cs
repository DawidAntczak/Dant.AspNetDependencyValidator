using Dant.AspNetDependencyValidator.Validation.Result;

namespace Dant.AspNetDependencyValidator.Validation
{
    public interface IValidationRunner
    {
        ValidationResult Run();
    }
}
