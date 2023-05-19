using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dant.AspNetDependencyValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        internal ValidationResult(IEnumerable<FailedValidation> failedValidations)
        {
            IsValid = failedValidations.All(x => x.Severity != Severity.Error);
            Message = BuildMessage(failedValidations);
        }

        private string BuildMessage(IEnumerable<FailedValidation> failedValidations)
        {
            var sb = new StringBuilder();
            foreach (var failedValidation in failedValidations)
            {
                sb.AppendLine($"{failedValidation.Severity}: {failedValidation.Message}");
            }
            return sb.ToString();
        }
    }
}
