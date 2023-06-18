using System;

namespace ServiceCollectionDIValidator.Validation.Result
{
    public sealed record FailedValidation(IssueType IssueType, Type ServiceType, string Message);
}
