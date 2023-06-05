using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Dant.AspNetDependencyValidator
{
    public static class AspNetDependenciesValidator
    {
        /// <summary>
        /// Validates the dependencies of a ASP.NET application using the default Microsoft.Extensions.DependencyInjection.IServiceCollection as DI container.
        /// It doesn't work for explicit 'serviceProvider.GetService&lt;T&gt;()' calls or e.g. IMiddleware dependencies.
        /// </summary>
        /// <typeparam name="TEntryPoint">A type included in the ASP.NET app assembly (e.g. &lt;Startup&gt;)</typeparam>
        /// <param name="additionalServicesToValidate">Additional services which should be retrieved from service provider.</param>
        /// <param name="validateServiceCollection">Use additional the default validation provided by servie provider builder (ValidateOnBuild and ValidateScopes).</param>
        public static ValidationResult Validate<TEntryPoint>(IEnumerable<Type> additionalServicesToValidate = null, bool validateServiceCollection = true) where TEntryPoint : class
        {
            ValidationResult validationResult = null;
            using (var app = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder => {
                    builder.ConfigureTestServices(serviceCollection =>
                    {
                        var dependencyValidator = new ServiceCollectionValidator(serviceCollection);

                        dependencyValidator.ValidateControllers(typeof(TEntryPoint).Assembly);
                        dependencyValidator.ValidatePages(typeof(TEntryPoint).Assembly);

                        if (additionalServicesToValidate != null)
                        {
                            dependencyValidator.ValidateServices(additionalServicesToValidate);
                        }

                        validationResult = new ValidationResult(dependencyValidator.FailedValidations);
                    });
                    builder.UseDefaultServiceProvider(options =>
                    {
                        options.ValidateScopes = validateServiceCollection;
#if NETCOREAPP3_1_OR_GREATER
                        options.ValidateOnBuild = validateServiceCollection;
#endif
                    });
                }))
            {
                app.CreateClient().Dispose();
            }

            return validationResult;
        }

        /// <summary>
        /// Validates the dependencies of a ASP.NET application using the default Microsoft.Extensions.DependencyInjection.IServiceCollection as DI container.
        /// It doesn't work for explicit 'serviceProvider.GetService&lt;T&gt;()' calls or e.g. IMiddleware dependencies.
        /// </summary>
        /// <param name="assemblyLocation">Location of the ASP.NET app assembly</param>
        /// <param name="additionalServicesToValidate">Additional services which should be retrieved from service provider.</param>
        /// <param name="validateServiceCollection">Use additional the default validation provided by servie provider builder (ValidateOnBuild and ValidateScopes).</param>
        public static ValidationResult Validate(string assemblyLocation, IEnumerable<Type> additionalServicesToValidate = null, bool validateServiceCollection = false)
        {
            var entryPoint = Assembly.LoadFrom(assemblyLocation)
                .GetTypes()
                .Where(t => t.IsClass)
                .First();

            var method = typeof(AspNetDependenciesValidator)
                .GetMethods()
                .Where(m => m.IsPublic && m.IsStatic && m.ContainsGenericParameters && m.Name == nameof(Validate))
                .Single()
                .MakeGenericMethod(entryPoint);

            return (ValidationResult)method.Invoke(null, new object[] { additionalServicesToValidate, validateServiceCollection });
        }
    }
}
