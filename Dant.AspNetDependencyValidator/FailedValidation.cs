using System;

namespace Dant.AspNetDependencyValidator
{
    public class FailedValidation
    {
        public FailureType FailureType { get; }
        public Type ServiceType { get; }

        public string Message { get; }

        public FailedValidation(FailureType failureType, Type serviceType, string message)
        {
            FailureType = failureType;
            ServiceType = serviceType;
            Message = message;
        }

        public override bool Equals(object obj)
        {
            if (obj is FailedValidation fv)
                return ServiceType == fv.ServiceType && Message == fv.Message && FailureType == fv.FailureType;

            return false;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode() ^ ServiceType.GetHashCode();
        }
    }
}
