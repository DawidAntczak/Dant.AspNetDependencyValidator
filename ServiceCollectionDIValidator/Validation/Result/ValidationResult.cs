using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCollectionDIValidator.Validation.Result
{
    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public IEnumerable<FailedValidation> FailedValidations { get; }

        internal ValidationResult(IEnumerable<FailedValidation> failedValidations)
        {
            IsValid = failedValidations.All(x => x.IssueType == IssueType.IncosistentLifetime);
            FailedValidations = failedValidations;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("IsValid: ").Append(IsValid).AppendLine();
            foreach (var failedValidation in FailedValidations.OrderBy(v => v.IssueType))
            {
                sb.Append(failedValidation.IssueType)
                    .Append(" for ")
                    .Append(failedValidation.ServiceType)
                    .Append(": ")
                    .AppendLine(failedValidation.Message);
            }
            return sb.ToString();
        }
    }
}
