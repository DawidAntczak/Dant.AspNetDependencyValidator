using System.Reflection;
using ServiceCollectionDIValidator;
using ServiceCollectionDIValidator.CodeAnalysis.CallRoutes;
using DependencyChecker.App;
using DependencyChecker.ExternalLib;
using DependencyChecker.SharedLib;

namespace DependencyChecker.Tests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void ValidatePagesDependencies()
    {
        var result = DIValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithValidation(including => including
                .Pages())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateDependencies()
    {
        var result = DIValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithValidation(including => including
                .Controllers())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateDependencies_OtherEntryUsage()
    {
        var result = DIValidator
            .ForEntryAssembly(typeof(WeatherForecast).Assembly)
            .WithValidation(including => including
                .Controllers())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateDependencies_WithSomeAssumedServices()
    {
        var result = DIValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithValidation(including => including
                .Controllers())
            .AssumingExistenceOf(services => services
                .Including<IGeoService>())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateDependencies_With_RequiredServicesGetFromProvider()
    {
        var result = DIValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithAdditional(assemblies => assemblies
                .Including(typeof(ServiceCollectionExtensions).Assembly))
            .WithValidation(including => including
                .Controllers()
                .TypesPassedToGetRequiredService())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void ValidateDependencies_With_RequiredServicesGetFromProvider_WithoutSugar()
    {
        var result = DIValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithAdditional(assemblies => assemblies
                .Including(typeof(ServiceCollectionExtensions).Assembly))
            .WithValidation(including => including
                .Controllers()
                .TypesPassed()
                    .To(typeof(ServiceProviderServiceExtensions)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Single(m => m.ContainsGenericParameters && m.Name == "GetRequiredService"))
                    .AtPosition(0))
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }
}
