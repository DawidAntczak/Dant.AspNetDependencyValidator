using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceCollectionDIValidator.Validation.Result;
using ServiceCollectionDIValidator.Validation.ValidationLogic;
using System;
using System.Collections.Generic;
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
            var failedValidations = new List<FailedValidation>();
            bool validationsRun = false;
            using var app = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(serviceCollection =>
                    {
                        RunValidations(serviceCollection, failedValidations);
                        validationsRun = true;
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
                failedValidations.Add(new FailedValidation(IssueType.BuildError, typeof(WebApplicationFactory<TEntryPoint>), $"App build failed with exception: {e}"));
            }
            if (!validationsRun && !failedValidations.Any())
            {
                throw new InvalidOperationException("Something went wrong: app build failed, but no failed validations were found.");
            }
            return new ValidationResult(failedValidations);
        }

        private void RunValidations(IServiceCollection serviceCollection, List<FailedValidation> failedValidations)
        {
            var validator = new Validator(serviceCollection, _assumedExistingTypes);
            foreach (var validation in _validations)
            {
                try
                {
                    validation(validator);
                }
                catch (Exception e)
                {
                    failedValidations.Add(new FailedValidation(IssueType.ValidationFailure, typeof(ValidationRunner<TEntryPoint>), $"Validation failed with exception: {e}"));
                }
            }
            failedValidations.AddRange(validator.FailedValidations);
        }
    }
}
