using Dant.AspNetDependencyValidator;

namespace EShopOnWeb.DependencyTests;

[TestFixture]
public class DependencyTest
{
    [Test]
    public void ValidatePublicApiProjectDependencies()
    {
        var result = ServiceCollectionValidator
            .ForEntryAssembly<Microsoft.eShopWeb.PublicApi.MappingProfile>()
            .WithValidation(including => including
                .Controllers()
                .TypesPassedToGetRequiredService())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateWebProjectDependencies()
    {
        var result = ServiceCollectionValidator
            .ForEntryAssembly<Microsoft.eShopWeb.Web.SlugifyParameterTransformer>()
            .WithValidation(including => including
                .Controllers()
                .TypesPassedToGetRequiredService())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }
}
