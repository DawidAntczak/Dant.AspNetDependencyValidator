using System;
using Mono.Cecil;

namespace ServiceCollectionDIValidator.CodeAnalysis.UsageFinder
{
    internal sealed record TypeUsage(Type UsedType, Type UsingType, MethodDefinition UsingMethod);
}
