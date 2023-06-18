namespace ServiceCollectionDIValidator.Validation.Result
{
    public enum IssueType
    {
        BuildError,
        MissingService,
        //EmptyIEnumerable,    not implemented
        IncosistentLifetime,
    }
}
