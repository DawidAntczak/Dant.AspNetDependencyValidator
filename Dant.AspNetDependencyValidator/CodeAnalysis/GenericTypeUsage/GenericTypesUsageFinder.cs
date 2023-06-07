using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dant.AspNetDependencyValidator.CodeAnalysis.GenericTypeUsage;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dant.AspNetDependencyValidator.CodeAnalysis.UsageFinder
{
    internal sealed class GenericTypesUsageFinder : IDisposable
    {
        private readonly AssemblyDefinition _assembly;

        public GenericTypesUsageFinder(string assemblyLocation)
        {
            _assembly = AssemblyDefinition.ReadAssembly(assemblyLocation);
        }

        public void Dispose()
        {
            _assembly.Dispose();
        }

        public IEnumerable<TypeUsage> FindUsedByMethodGenericTypes(MethodInfo methodWithGenericParameter, int parameterPosition)
        {
            if (!methodWithGenericParameter.ContainsGenericParameters)
                throw new ArgumentException("Method doesn't contain generic parameters", nameof(methodWithGenericParameter));

            var methodToBeFoundRef = _assembly.MainModule.ImportReference(methodWithGenericParameter);

            if (parameterPosition > methodToBeFoundRef.GenericParameters.Count)
                throw new ArgumentException($"Method contains only {methodToBeFoundRef.GenericParameters.Count} parameter but requested position {parameterPosition}", nameof(methodWithGenericParameter));

            var callsStacksToMethod = new List<TypeUsage>();

            // Types property doesn't include lambdas so use GetTypes() instead but GetMethods() returns less than Methods property
            foreach (var type in _assembly.MainModule.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    // Find calls to the method
                    var calls = FindGenericTypesUsedToCallMethod(methodToBeFoundRef, parameterPosition, type, method);
                    callsStacksToMethod.AddRange(calls);
                }
            }
            var allAssemblyMethods = _assembly.MainModule.GetTypes().SelectMany(t => t.Methods);

            return callsStacksToMethod;
        }

        // Helper method to find calls to a given method in an assembly
        private List<TypeUsage> FindGenericTypesUsedToCallMethod(MethodReference methodToFind, int parameterPosition, TypeDefinition callingType, MethodDefinition callingMethod)
        {
            if (!callingMethod.HasBody)
                return new List<TypeUsage>();

            var result = new List<TypeUsage>();

            foreach (var instruction in callingMethod.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                {
                    if (instruction.Operand is GenericInstanceMethod calledMethod && calledMethod.Name == methodToFind.Name)
                    {
                        var arg = calledMethod.GenericArguments.ElementAt(parameterPosition);
                        result.Add(new TypeUsage(arg.ConvertToSystemType(), callingType.ConvertToSystemType(), callingMethod));
                    }
                }
            }
            return result;
        }
    }
}

