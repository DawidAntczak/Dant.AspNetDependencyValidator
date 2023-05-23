using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dant.AspNetDependencyValidator.SourceAnalysis
{
    public static class CallsFinder
    {
        public static IEnumerable<IEnumerable<MethodDefinition>> FindCallsToGetService(string pathToAssembly, int maxDepth = 5)
        {
            var assembly = AssemblyDefinition.ReadAssembly(pathToAssembly);

            // Get the method reference
            var methodToBeFound = typeof(IServiceProvider).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Single(m => m.Name.Contains("GetService"));
            var methodToBeFoundRef = assembly.MainModule.ImportReference(methodToBeFound);

            //var extensionMethods = typeof(ServiceProviderServiceExtensions).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            //var methods = extensionMethods.Append(directMethod);

            var allAssemblyMethods = assembly.MainModule.Types.SelectMany(t => t.Methods);

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
        static List<List<MethodDefinition>> FindCallsToMethod(MethodReference methodToFind, IEnumerable<MethodDefinition> callStack, int maxDepth)
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
                        var parameter = instruction.Previous.Operand as ParameterReference;
                        result.Add(callStack.Append(calledMethod.Resolve()).ToList());
                    }
                    var resolvedCalledMethod = calledMethod.Resolve();
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
