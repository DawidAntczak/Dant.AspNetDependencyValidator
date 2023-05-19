using System;

namespace Dant.AspNetDependencyValidator
{
    internal class FailedValidation
    {
        public Severity Severity { get; }
        public Type ServiceType { get; }

        public string Message { get; }

        public FailedValidation(Severity severity, Type serviceType, string message)
        {
            Severity = severity;
            ServiceType = serviceType;
            Message = message;
        }

        public override bool Equals(object obj)
        {
            if (obj is FailedValidation fv)
                return ServiceType == fv.ServiceType && Message == fv.Message && Severity == fv.Severity;

            return false;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode() ^ ServiceType.GetHashCode();
        }
    }
}
