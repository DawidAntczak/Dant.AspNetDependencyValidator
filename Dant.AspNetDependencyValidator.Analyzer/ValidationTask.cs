using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace Dant.AspNetDependencyValidator.Analyzer
{
    public class ValidationTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string AssemblyPath { get; set; }

        public override bool Execute()
        {
            //Log.LogMessage(MessageImportance.High, $"------------- EXECUTING {nameof(ValidationTask)} in version {Assembly.GetExecutingAssembly().GetName().Version} for assembly path {AssemblyPath} -------------");

            var validatorDllPath = Path.Combine(Path.GetDirectoryName(AssemblyPath), "Dant.AspNetDependencyValidator.dll");

            var validatorDomain = AppDomain.CreateDomain("ValidatorDomain");

            //validatorDomain.Load(File.ReadAllBytes(typeof(ValidationTask).Assembly.Location));

            AppDomain.CurrentDomain.AssemblyResolve += ValidatorDomain_AssemblyResolve; // new Resolver(Path.GetDirectoryName(AssemblyPath)).ValidatorDomain_AssemblyResolve;

            var validatorAssemblyRef = typeof(AspNetDependenciesValidator).Assembly;
            //validatorAssemblyRef.GetReferencedAssemblies().ToList().ForEach(a => validatorDomain.Load(a));
            var validatorAssembly = validatorDomain.Load(validatorAssemblyRef.GetName());

            //Log.LogMessage(MessageImportance.High, "Loaded validator assembly into separate domain");

            var validatorClass = validatorAssembly.GetType("Dant.AspNetDependencyValidator.AspNetDependenciesValidator", true);

            var apiEntryPoint = validatorDomain.Load(AssemblyName.GetAssemblyName(AssemblyPath))
                .GetTypes()
                .Where(t => t.IsClass)
                .First();

            var method = validatorAssembly.GetType("Dant.AspNetDependencyValidator.AspNetDependenciesValidator")
                .GetMethods()
                .Where(m => m.IsPublic && m.IsStatic && m.ContainsGenericParameters && m.Name == "Validate")
                .Single()
                .MakeGenericMethod(apiEntryPoint);

            Log.LogMessage(MessageImportance.High, "Invoking validation method");

            dynamic result = method.Invoke(null, new object[] { null, null });

            Log.LogMessage(MessageImportance.High, result.Message);

            return result.IsValid;
        }

        private Assembly ValidatorDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyPath = Path.Combine(Path.GetDirectoryName(AssemblyPath), new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                return null;
            }
            return Assembly.LoadFrom(assemblyPath);
        }
    }
}
