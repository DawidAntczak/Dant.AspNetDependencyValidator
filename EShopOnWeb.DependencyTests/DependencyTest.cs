using Dant.AspNetDependencyValidator;

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
        var result = AspNetDependenciesValidator.Validate<Microsoft.eShopWeb.PublicApi.MappingProfile>(validateServiceCollection: true);
        Console.WriteLine(result.Message);
        Assert.That(result.IsValid, Is.True, result.Message);
    }

    [Test]
    public void ValidateWebDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<Microsoft.eShopWeb.Web.SlugifyParameterTransformer>(validateServiceCollection: true);
        Console.WriteLine(result.Message);
        Assert.That(result.IsValid, Is.True, result.Message);
    }
}
