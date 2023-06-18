using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using ServiceCollectionDIValidator.Validation.ValidationLogic;
using ServiceCollectionDIValidator.CodeAnalysis.UsageFinder;
using ServiceCollectionDIValidator.Extensions;
using ServiceCollectionDIValidator.Validation.Builder.Stages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ServiceCollectionDIValidator.Validation.Builder.AddAssembliesStage
{
    public interface IValidationCollectionBuilder
    {
        IValidationCollectionBuilder Controllers();
        IValidationCollectionBuilder Pages();
        IValidationCollectionBuilder EntryPointsOfType<T>();
        IValidationCollectionBuilder EntryPointsOfType(Type type);
        IValidationCollectionBuilder EntryPointOfExactType<T>();
        IValidationCollectionBuilder EntryPointOfExactType(Type type);
        ITypesPassedMethodStage TypesPassed();
        IValidationCollectionBuilder TypesPassedToGetRequiredService();
        IValidationCollectionBuilder TypesPassedToGetService();
    }

    internal sealed class ValidationCollectionBuilder : IValidationCollectionBuilder
    {
        public List<Action<Validator>> Validations { get; } = new List<Action<Validator>>();

        private readonly IEnumerable<Assembly> _assembliesToValidate;

        public ValidationCollectionBuilder(IEnumerable<Assembly> assembliesToValidate)
        {
            _assembliesToValidate = assembliesToValidate;

            Middlewares();
        }

        public IValidationCollectionBuilder Controllers()
        {
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, typeof(ControllerBase)));
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, typeof(IActionFilter)));
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, typeof(IAsyncActionFilter)));
            return this;
        }

        public IValidationCollectionBuilder Pages()
        {
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, Type.GetType("Microsoft.AspNetCore.Mvc.RazorPages.PageModel, Microsoft.AspNetCore.Mvc.RazorPages", true)));
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, Type.GetType("Microsoft.AspNetCore.Mvc.Filters.IPageFilter, Microsoft.AspNetCore.Mvc.RazorPages", true)));
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, Type.GetType("Microsoft.AspNetCore.Mvc.Filters.IAsyncPageFilter, Microsoft.AspNetCore.Mvc.RazorPages", true)));
            Validations.Add(v => v.ValidatePropertiesInjection(_assembliesToValidate,
                Type.GetType("Microsoft.AspNetCore.Components.IComponent, Microsoft.AspNetCore.Components", true),
                Type.GetType("Microsoft.AspNetCore.Components.InjectAttribute, Microsoft.AspNetCore.Components", true)));

            return this;
        }

        public IValidationCollectionBuilder EntryPointsOfType<T>()
        {
            return EntryPointsOfType(typeof(T));
        }

        public IValidationCollectionBuilder EntryPointsOfType(Type type)
        {
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, type));
            return this;
        }

        public IValidationCollectionBuilder EntryPointOfExactType<T>()
        {
            return EntryPointOfExactType(typeof(T));
        }

        public IValidationCollectionBuilder EntryPointOfExactType(Type type)
        {
            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, new[] { type }));
            return this;
        }

        public ITypesPassedMethodStage TypesPassed()
        {
            var onFinish = (TypesPassedBuilder builder) =>
            {
                var requiredTypes = GetTypesPassedTo(builder.MethodWithGenericParameters, builder.ParameterPosition);
                Validations.Add(v => v.ValidateServices(requiredTypes));
            };
            return new TypesPassedBuilder(this, onFinish);
        }

        public IValidationCollectionBuilder TypesPassedToGetRequiredService()
        {
            var method = typeof(ServiceProviderServiceExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.ContainsGenericParameters && m.Name == "GetRequiredService");
            return TypesPassed().To(method).AtPosition(0);
        }

        public IValidationCollectionBuilder TypesPassedToGetService()
        {
            var method = typeof(ServiceProviderServiceExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.ContainsGenericParameters && m.Name == "GetService");
            return TypesPassed().To(method).AtPosition(0);
        }

        /* TODO: Implement
        public ValidationBuilder<TEntryPoint> ValidateLifetime()
        {
            return this;
        }*/

        private ValidationCollectionBuilder Middlewares()
        {
            var registeringMethod = typeof(UseMiddlewareExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.ContainsGenericParameters && m.Name == "UseMiddleware");

            var requiredTypes = GetTypesPassedTo(registeringMethod, 0);

            Validations.Add(v => v.ValidateEntryPoints(_assembliesToValidate, requiredTypes));
            return this;
        }

        private IEnumerable<Type> GetTypesPassedTo(MethodInfo MethodWithGenericParameters, int position)
        {
            using var usageFinders = _assembliesToValidate
                .Select(a => new GenericTypesUsageFinder(a.Location))
                .ToDisposableArray();

            var usages = usageFinders
                .SelectMany(f => f.FindUsedByMethodGenericTypes(MethodWithGenericParameters, position));

            return usages
                .Select(u => u.UsedType)
                .Distinct()
                .ToArray();
        }
    }
}
