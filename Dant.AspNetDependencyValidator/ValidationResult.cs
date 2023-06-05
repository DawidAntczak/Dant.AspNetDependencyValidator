using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dant.AspNetDependencyValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public IEnumerable<FailedValidation> FailedValidations { get; }

        internal ValidationResult(IEnumerable<FailedValidation> failedValidations)
        {
            IsValid = failedValidations.All(x => x.FailureType != FailureType.MissingService);
            FailedValidations = failedValidations;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"IsValid: {IsValid}");
            foreach (var failedValidation in FailedValidations.OrderByDescending(v => v.FailureType))
            {
                sb.Append(failedValidation.FailureType)
                    .Append(": ")
                    .AppendLine(failedValidation.Message);
            }
            return sb.ToString();
        }
    }
}
