using System;

namespace Dant.AspNetDependencyValidator.Validation.Result
{
    public sealed record FailedValidation(IssueType IssueType, Type ServiceType, string Message);
}
