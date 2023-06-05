using System.Reflection;
using Dant.AspNetDependencyValidator;
using Dant.AspNetDependencyValidator.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace EShopOnWeb.DependencyTests;

[TestFixture]
public class DependencyTest
{
    /*[Test]
    public void ValidateBlazorAdminDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<BlazorAdmin.CustomAuthStateProvider>(validateServiceCollection: true);
        Console.WriteLine(result.Message);
        Assert.That(result.IsValid, Is.True, result.Message);
    }*/

    [Test]
    public void ValidatePublicApiDependencies()
    {
        var genericMethod = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.ContainsGenericParameters && m.Name == "GetRequiredService");

        using var genericTypesFinder = new GenericTypesUsageFinder(typeof(Microsoft.eShopWeb.PublicApi.MappingProfile).Assembly.Location);

        var usedGenericTypes = genericTypesFinder.FindUsedByMethodGenericTypes(genericMethod);

        var result = AspNetDependenciesValidator.Validate<Microsoft.eShopWeb.PublicApi.MappingProfile>(usedGenericTypes.Select(t => t.UsedType));
        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateWebDependencies()
    {
        var genericMethod = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
           .Single(m => m.ContainsGenericParameters && m.Name == "GetRequiredService");

        using var genericTypesFinder = new GenericTypesUsageFinder(typeof(Microsoft.eShopWeb.Web.SlugifyParameterTransformer).Assembly.Location);

        var usedGenericTypes = genericTypesFinder.FindUsedByMethodGenericTypes(genericMethod);

        var result = AspNetDependenciesValidator.Validate<Microsoft.eShopWeb.Web.SlugifyParameterTransformer>(usedGenericTypes.Select(t => t.UsedType));
        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }
}
