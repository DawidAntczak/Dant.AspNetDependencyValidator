using System;

namespace Dant.AspNetDependencyValidator
{
    public sealed class FailedValidation
    {
        public IssueType IssueType { get; }
        public Type ServiceType { get; }
        public string Message { get; }

        public FailedValidation(IssueType issueType, Type serviceType, string message)
        {
            IssueType = issueType;
            ServiceType = serviceType;
            Message = message;
        }

        public override bool Equals(object obj)
        {
            if (obj is FailedValidation fv)
                return ServiceType == fv.ServiceType && Message == fv.Message && IssueType == fv.IssueType;

            return false;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode() ^ ServiceType.GetHashCode();
        }
    }
}
