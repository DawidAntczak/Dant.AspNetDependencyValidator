using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dant.AspNetDependencyValidator.CodeAnalysis
{
    public class GenericTypesFinder : IDisposable
    {
        private readonly AssemblyDefinition _assembly;

        public GenericTypesFinder(string assemblyLocation)
        {
            _assembly = AssemblyDefinition.ReadAssembly(assemblyLocation);
        }

        public void Dispose()
        {
            _assembly.Dispose();
        }

        public IEnumerable<TypeUsage> FindUsedByMethodGenericTypes(MethodInfo methodWithGenericParameter)
        {
            if (!methodWithGenericParameter.ContainsGenericParameters)
                throw new ArgumentException("Method doesn't contain generic parameters", nameof(methodWithGenericParameter));

            var methodToBeFoundRef = _assembly.MainModule.ImportReference(methodWithGenericParameter);

            var callsStacksToMethod = new List<TypeUsage>();

            // Types property doesn't include lambdas so use GetTypes() instead but GetMethods() returns less than Methods property
            foreach (var type in _assembly.MainModule.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    // Find calls to the method
                    var calls = FindGenericTypesUsedToCallMethod(methodToBeFoundRef, type, method);
                    callsStacksToMethod.AddRange(calls);
                }
            }
            var allAssemblyMethods = _assembly.MainModule.GetTypes().SelectMany(t => t.Methods);

            return callsStacksToMethod;
        }

        // Helper method to find calls to a given method in an assembly
        private List<TypeUsage> FindGenericTypesUsedToCallMethod(MethodReference methodToFind, TypeDefinition callingType, MethodDefinition callingMethod)
        {
            if (!callingMethod.HasBody)
                return new List<TypeUsage>();

            var result = new List<TypeUsage>();

            foreach (var instruction in callingMethod.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                {
                    var calledMethod = instruction.Operand as GenericInstanceMethod;

                    if (calledMethod != null && calledMethod.Name == methodToFind.Name)
                    {
                        var arg = calledMethod.GenericArguments.First();
                        result.Add(new TypeUsage(arg.ConvertToSystemType(), callingType.ConvertToSystemType(), callingMethod));
                    }
                }
            }
            return result;
        }
    }
}
