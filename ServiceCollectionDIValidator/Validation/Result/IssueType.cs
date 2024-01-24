namespace ServiceCollectionDIValidator.Validation.Result
{
    public enum IssueType
    {
        BuildError,
        ValidationFailure,
        MissingService,
        //EmptyIEnumerable,    not implemented
        IncosistentLifetime,
    }
}
