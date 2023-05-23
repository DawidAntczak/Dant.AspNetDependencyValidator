using System;
using System.Linq;
using Mono.Cecil;

namespace Dant.AspNetDependencyValidator.CodeAnalysis
{
    public static class TypeReferenceExtensions
    {
        public static Type ConvertToSystemType(this TypeReference type)
        {
            var reflectionName = type.IsGenericInstance
                ? type.GetFullName()
                : $"{type.FullDeclaringName()}, {type.GetAssemblyName()}";

            return Type.GetType(reflectionName, true);
        }

        private static string FullDeclaringName(this TypeReference type)
        {
            return type.DeclaringType != null
                ? type.DeclaringType.FullName
                : type.FullName;
        }

        private static string GetFullName(this TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)type;
                return string.Format("{0}.{1}[[{2}]]", genericInstance.Namespace, type.Name, string.Join(",", genericInstance.GenericArguments.Select(p => p.GetFullName()).ToArray())) + ", " + genericInstance.Scope.Name;
            }
            return type.FullName + ", " + type.GetAssemblyName();
        }

        private static string GetAssemblyName(this TypeReference type)
        {
            return type.Scope is ModuleDefinition moduleScope ? moduleScope.Assembly.FullName : type.Scope.Name;
        }
    }
}
