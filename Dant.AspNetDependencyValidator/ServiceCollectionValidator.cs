using System.Linq;
using System.Reflection;
using Dant.AspNetDependencyValidator.Builder;

namespace Dant.AspNetDependencyValidator
{
    public static class ServiceCollectionValidator
    {
        private static readonly MethodInfo MainMethod = typeof(ServiceCollectionValidator)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.ContainsGenericParameters && m.Name == nameof(ForEntryAssembly));

        public static IAddAssembliesBuildStage ForEntryAssembly<TEntryPoint>() where TEntryPoint : class
        {
            return new ValidationRunnerBuilder<TEntryPoint>();
        }

        public static IAddAssembliesBuildStage ForEntryAssembly(Assembly assembly)
        {
            var entryPoint = assembly.GetTypes().First(t => t.IsClass);
            var methodWithAppliedType = MainMethod.MakeGenericMethod(entryPoint);
            return (IAddAssembliesBuildStage)methodWithAppliedType.Invoke(null, null);
        }
    }
}
