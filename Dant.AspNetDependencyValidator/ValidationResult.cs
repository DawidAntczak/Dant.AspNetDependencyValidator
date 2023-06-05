using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dant.AspNetDependencyValidator
{
    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public IEnumerable<FailedValidation> FailedValidations { get; }

        internal ValidationResult(IEnumerable<FailedValidation> failedValidations)
        {
            IsValid = failedValidations.All(x => x.IssueType != IssueType.MissingService);
            FailedValidations = failedValidations;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"IsValid: {IsValid}");
            foreach (var failedValidation in FailedValidations.OrderByDescending(v => v.IssueType))
            {
                sb.Append(failedValidation.IssueType)
                    .Append(": ")
                    .AppendLine(failedValidation.Message);
            }
            return sb.ToString();
        }
    }
}
