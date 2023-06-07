using System;
using Mono.Cecil;

namespace Dant.AspNetDependencyValidator.CodeAnalysis.UsageFinder
{
    internal sealed class TypeUsage
    {
        public Type UsedType { get; }
        public Type UsingType { get; }
        public MethodDefinition UsingMethod { get; }

        public TypeUsage(Type usedType, Type usingType, MethodDefinition usingMethod)
        {
            UsedType = usedType;
            UsingType = usingType;
            UsingMethod = usingMethod;
        }
    }
}
