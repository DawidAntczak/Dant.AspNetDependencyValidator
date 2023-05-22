using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dant.AspNetDependencyValidator.SourceAnalysis
{
    public static class CallsFinder
    {
        public static IEnumerable<InvocationExpressionSyntax> FindCalls(string pathToAssembly)
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
            var callsToMethod = new List<MethodReference>();
            foreach (var method in allAssemblyMethods)
            {
                var calls = FindCallsToMethod(methodToBeFoundRef, new List<MethodDefinition> { method });
                //Console.WriteLine($"Found {calls.Count} calls to {methodToBeFoundRef}");
                callsToMethod.AddRange(calls);
            }

            foreach (var call in callsToMethod)
            {
                Console.WriteLine($"- {call}");
            }

            return new List<InvocationExpressionSyntax>();
        }

        // Helper method to find calls to a given method in an assembly
        static List<MethodReference> FindCallsToMethod(MethodReference methodToFind, IEnumerable<MethodDefinition> callStack)
        {
            var methodToSearch = callStack.Last();
            if (callStack.Count() > 10 || !methodToSearch.HasBody)
                return new List<MethodReference>();

            var result = new List<MethodReference>();

            foreach (var instruction in methodToSearch.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                {
                    var calledMethod = instruction.Operand as MethodReference;

                    if (calledMethod != null && calledMethod.FullName == methodToFind.FullName)
                    {
                        var parameter = instruction.Previous.Operand as ParameterReference;
                        //result.Add(string.Join(" -> ", callStack.Select(x => $"{x.DeclaringType.FullName}.{x.Name}")));
                        result.Add(calledMethod);
                    }
                    var resolvedCalledMethod = calledMethod.Resolve();
                    if (resolvedCalledMethod.Parameters.Select(p => p.ParameterType.FullName).Contains(typeof(IServiceProvider).FullName))
                    {
                        result.AddRange(FindCallsToMethod(methodToFind, callStack.Append(calledMethod.Resolve())));
                    }
                }
            }
            return result;
        }
    }
}
