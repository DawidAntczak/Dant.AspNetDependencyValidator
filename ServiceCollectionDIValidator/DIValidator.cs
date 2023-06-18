using System.Linq;
using System.Reflection;
using ServiceCollectionDIValidator.Builder;

namespace ServiceCollectionDIValidator
{
    public static class DIValidator
    {
        private static readonly MethodInfo MainForEntryAssemblyMethod = typeof(DIValidator)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.ContainsGenericParameters && m.Name == nameof(ForEntryAssembly));

        public static IAddAssembliesBuildStage ForEntryAssembly<TEntryPoint>() where TEntryPoint : class
        {
            return new ValidationRunnerBuilder<TEntryPoint>();
        }

        public static IAddAssembliesBuildStage ForEntryAssembly(Assembly assembly)
        {
            var entryPoint = assembly.GetTypes().First(t => t.IsClass);
            var methodWithAppliedType = MainForEntryAssemblyMethod.MakeGenericMethod(entryPoint);
            return (IAddAssembliesBuildStage)methodWithAppliedType.Invoke(null, null);
        }
    }
}
