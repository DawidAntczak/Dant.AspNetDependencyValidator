using ServiceCollectionDIValidator;

namespace EShopOnWeb.DependencyTests;

[TestFixture]
public class DependencyTest
{
    [Test]
    public void ValidatePublicApiProjectDependencies()
    {
        var result = DIValidator
            .ForEntryAssembly<Microsoft.eShopWeb.PublicApi.MappingProfile>()
            .WithValidation(including => including
                .Controllers()
                .Pages()
                .TypesPassedToGetRequiredService())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateWebProjectDependencies()
    {
        var result = DIValidator
            .ForEntryAssembly<Microsoft.eShopWeb.Web.SlugifyParameterTransformer>()
            .WithAdditional(assemblies => assemblies
                .Including<BlazorAdmin.CustomAuthStateProvider>())
            .WithValidation(including => including
                .Controllers()
                .Pages()
                .TypesPassedToGetRequiredService())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }
}
