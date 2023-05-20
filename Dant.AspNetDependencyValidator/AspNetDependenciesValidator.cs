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
        /// <param name="validateServiceCollection">Informs if the in service collection as a whole should be validated.
        /// Default value of 'false' instructs to only validate recurrently the dependencies used by controllers.
        /// Disable if it causes problems because of many nuances which may not be validated correctly in the current version.</param>
        public static ValidationResult Validate<TEntryPoint>(bool validateServiceCollection = false) where TEntryPoint : class
        {
            ValidationResult validationResult = null;
            using (var app = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder => builder.ConfigureTestServices(serviceCollection =>
                {
                    var dependencyValidator = new ServiceCollectionValidator(serviceCollection);

                    // Validate entire collection (but this does not validate controllers)
                    if (validateServiceCollection)
                    {
                        dependencyValidator.ValidateServiceCollection();
                    }

                    // Scan an assembly and validate all controllers, both the constructor injected dependencies and dependencies injected into endpoints 
                    // marked with FromService attribute
                    dependencyValidator.ValidateControllers(typeof(TEntryPoint).Assembly);
                    dependencyValidator.ValidatePages(typeof(TEntryPoint).Assembly);

                    validationResult = new ValidationResult(dependencyValidator.FailedValidations);
                })))
            {
                app.CreateClient().Dispose();
            }

            return validationResult;
        }
    }
}
