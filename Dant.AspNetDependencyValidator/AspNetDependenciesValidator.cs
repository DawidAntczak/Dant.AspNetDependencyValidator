using System.Linq;
using System.Reflection;

namespace Dant.AspNetDependencyValidator
{
    public static class AspNetDependenciesValidator
    {
        /// <summary>
        /// Validates the dependencies of a ASP.NET application using the default Microsoft.Extensions.DependencyInjection.IServiceCollection as DI container.
        /// It doesn't work for explicit 'serviceProvider.GetService&lt;T&gt;()' calls or e.g. IMiddleware dependencies.
        /// </summary>
        /// <typeparam name="TEntryPoint">A type included in the ASP.NET app assembly (e.g. &lt;Startup&gt;)</typeparam>
        /// <param name="additionalAssemblies">Other assemblies to scan</param>
        public static ValidationResult Validate<TEntryPoint>(params Assembly[] additionalAssemblies) where TEntryPoint : class
        {
            var builder = ServiceCollectionValidator<TEntryPoint>.ForEntryAssembly();

            foreach (var assembly in additionalAssemblies)
            {
                builder.And(assembly);
            }

            return builder
                .WithValidationsOf()
                .Controllers()
                .Pages()
                .Build()
                .Run();
        }

        /// <summary>
        /// Validates the dependencies of a ASP.NET application using the default Microsoft.Extensions.DependencyInjection.IServiceCollection as DI container.
        /// It doesn't work for explicit 'serviceProvider.GetService&lt;T&gt;()' calls or e.g. IMiddleware dependencies.
        /// </summary>
        /// <param name="mainAssemblyLocation">Location of the ASP.NET app assembly</param>
        /// <param name="additionalAssembliesLocations">Locations of other assemblies to scan</param>
        public static ValidationResult Validate(string mainAssemblyLocation, params string[] additionalAssembliesLocations)
        {
            var entryPoint = Assembly.LoadFrom(mainAssemblyLocation)
                .GetTypes()
                .Where(t => t.IsClass)
                .First();

            var method = typeof(AspNetDependenciesValidator)
                .GetMethods()
                .Where(m => m.IsPublic && m.IsStatic && m.ContainsGenericParameters && m.Name == nameof(Validate))
                .Single()
                .MakeGenericMethod(entryPoint);

            var additionalAssemblies = additionalAssembliesLocations.Select(path => Assembly.LoadFrom(path)).ToArray();

            return (ValidationResult)method.Invoke(null, new object[] { additionalAssemblies });
        }
    }
}
