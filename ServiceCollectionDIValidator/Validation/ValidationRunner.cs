using System;
using System.Collections.Generic;
using ServiceCollectionDIValidator.Validation.Result;
using ServiceCollectionDIValidator.Validation.ValidationLogic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System.Linq;

namespace ServiceCollectionDIValidator.Validation
{
    internal class ValidationRunner<TEntryPoint> : IValidationRunner where TEntryPoint : class
    {
        private readonly IEnumerable<Action<Validator>> _validations;
        private readonly HashSet<Type> _assumedExistingTypes;

        public ValidationRunner(IEnumerable<Action<Validator>> validations, HashSet<Type> assumedExistingTypes)
        {
            _validations = validations;
            _assumedExistingTypes = assumedExistingTypes;
        }

        public ValidationResult Run()
        {
            HashSet<FailedValidation> failedValidations = null;
            using var app = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(serviceCollection =>
                    {
                        var validator = new Validator(serviceCollection, _assumedExistingTypes);
                        foreach (var validation in _validations)
                        {
                            validation(validator);
                        }
                        failedValidations = validator.FailedValidations;
                    });

                    builder.UseDefaultServiceProvider(options =>
                    {
                        options.ValidateScopes = true;
#if NETCOREAPP3_1_OR_GREATER
                        options.ValidateOnBuild = true;
#endif
                    });
                });
            try
            {
                using var client = app.CreateClient();
            }
            catch (Exception e)
            {
                failedValidations ??= new HashSet<FailedValidation>();
                failedValidations.Add(new FailedValidation(IssueType.BuildError, typeof(WebApplicationFactory<TEntryPoint>), $"App build failed with exception: {e.Message}"));
            }
            if (failedValidations == null)
            {
                throw new InvalidOperationException("Validation failed, but no failed validations were found.");
            }
            return new ValidationResult(failedValidations);
        }
    }
}
