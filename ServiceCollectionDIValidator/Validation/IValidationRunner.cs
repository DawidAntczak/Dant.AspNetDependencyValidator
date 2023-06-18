using ServiceCollectionDIValidator.Validation.Result;

namespace ServiceCollectionDIValidator.Validation
{
    public interface IValidationRunner
    {
        ValidationResult Run();
    }
}
