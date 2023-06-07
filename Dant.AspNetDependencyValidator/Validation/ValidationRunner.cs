using System;
using System.Collections.Generic;
using Dant.AspNetDependencyValidator.Validation.Result;
using Dant.AspNetDependencyValidator.Validation.ValidationLogic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Dant.AspNetDependencyValidator.Validation
{
    internal class ValidationRunner<TEntryPoint> : IValidationRunner where TEntryPoint : class
    {
        private readonly IEnumerable<Action<Validator>> _validations;
        private readonly bool _onBuildValidation;
        private readonly HashSet<Type> _assumedExistingTypes;

        public ValidationRunner(IEnumerable<Action<Validator>> validations, bool onBuildValidation, HashSet<Type> assumedExistingTypes)
        {
            _validations = validations;
            _onBuildValidation = onBuildValidation;
            _assumedExistingTypes = assumedExistingTypes;
        }

        public ValidationResult Run()
        {
            ValidationResult validationResult = null;
            using (var app = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(serviceCollection =>
                    {
                        var validator = new Validator(serviceCollection, _assumedExistingTypes);
                        foreach (var validation in _validations)
                        {
                            validation(validator);
                        }
                        validationResult = new ValidationResult(validator.FailedValidations);
                    });

                    builder.UseDefaultServiceProvider(options =>
                    {
                        options.ValidateScopes = _onBuildValidation;
#if NETCOREAPP3_1_OR_GREATER
                        options.ValidateOnBuild = _onBuildValidation;
#endif
                    });
                }))
            {
                using (var client = app.CreateClient())
                {
                }
            }
            return validationResult;
        }
    }
}

