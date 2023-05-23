using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dant.AspNetDependencyValidator.CodeAnalysis
{
    public class MethodCallsFinder : IDisposable
    {
        private readonly AssemblyDefinition _assembly;

        public MethodCallsFinder(string assemblyLocation)
        {
            _assembly = AssemblyDefinition.ReadAssembly(assemblyLocation);
        }

        public void Dispose()
        {
            _assembly.Dispose();
        }

        public IEnumerable<IEnumerable<MethodDefinition>> FindCallRoutesTo(MethodInfo methodToFind, int maxDepth = 5)
        {
            var methodToBeFoundRef = _assembly.MainModule.ImportReference(methodToFind);

            // Types property doesn't include lambdas so use GetTypes() instead but GetMethods() returns less than Methods property
            var allAssemblyMethods = _assembly.MainModule.GetTypes().SelectMany(t => t.Methods);

            // Find calls to the method
            var callsStacksToMethod = new List<List<MethodDefinition>>();
            foreach (var method in allAssemblyMethods)
            {
                var calls = FindCallsToMethod(methodToBeFoundRef, new List<MethodDefinition> { method }, maxDepth);
                callsStacksToMethod.AddRange(calls);
            }

            return callsStacksToMethod;
        }

        // Helper method to find calls to a given method in an assembly
        private List<List<MethodDefinition>> FindCallsToMethod(MethodReference methodToFind, IEnumerable<MethodDefinition> callStack, int maxDepth)
        {
            var methodToSearch = callStack.Last();
            if (callStack.Count() > maxDepth || !methodToSearch.HasBody)
                return new List<List<MethodDefinition>>();

            var result = new List<List<MethodDefinition>>();

            foreach (var instruction in methodToSearch.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                {
                    var calledMethod = instruction.Operand as MethodReference;

                    if (calledMethod != null && calledMethod.FullName == methodToFind.FullName)
                    {
                        result.Add(callStack.Append(calledMethod.Resolve()).ToList());
                    }
                    var resolvedCalledMethod = calledMethod.Resolve();
                    // TODO rethink if this check has no holes - but some checks need to be done to avoid long time running
                    if (resolvedCalledMethod.Parameters.Select(p => p.ParameterType.FullName).Contains(typeof(IServiceProvider).FullName))
                    {
                        result.AddRange(FindCallsToMethod(methodToFind, callStack.Append(calledMethod.Resolve()), maxDepth));
                    }
                }
            }
            return result;
        }
    }
}
