using Dant.AspNetDependencyValidator;

namespace EShopOnWeb.DependencyTests;

[TestFixture]
public class DependencyTest
{
    [Test]
    public void ValidatePublicApiProjectDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<Microsoft.eShopWeb.PublicApi.MappingProfile>();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateWebProjectDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<Microsoft.eShopWeb.Web.SlugifyParameterTransformer>();
        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }
}
