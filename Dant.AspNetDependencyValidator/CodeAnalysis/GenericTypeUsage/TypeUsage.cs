using System;
using Mono.Cecil;

namespace Dant.AspNetDependencyValidator.CodeAnalysis.UsageFinder
{
    internal sealed record TypeUsage(Type UsedType, Type UsingType, MethodDefinition UsingMethod);
}
