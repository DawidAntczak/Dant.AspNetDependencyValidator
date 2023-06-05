using System;
using System.Collections.Generic;
using Dant.AspNetDependencyValidator.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Dant.AspNetDependencyValidator
{
    public interface IValidator
    {
        ValidationResult Run();
    }

    internal class Validator<TEntryPoint> : IValidator where TEntryPoint : class
    {
        private readonly IEnumerable<Action<ServiceCollectionValidator>> _validations;
        private readonly bool _onBuildValidation;
        private readonly HashSet<Type> _assumedExistingTypes;

        public Validator(IEnumerable<Action<ServiceCollectionValidator>> validations, bool onBuildValidation, HashSet<Type> assumedExistingTypes)
        {
            _validations = validations;
            _onBuildValidation = onBuildValidation;
            _assumedExistingTypes = assumedExistingTypes;
        }

        public ValidationResult Run()
        {
            ValidationResult validationResult = null;
            using (var app = new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder => {
                    builder.ConfigureTestServices(serviceCollection =>
                    {
                        var validator = new ServiceCollectionValidator(serviceCollection, _assumedExistingTypes);
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
