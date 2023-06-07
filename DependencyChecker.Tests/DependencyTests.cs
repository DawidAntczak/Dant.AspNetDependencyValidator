using System.Reflection;
using Dant.AspNetDependencyValidator;
using Dant.AspNetDependencyValidator.CodeAnalysis.CallRoutes;
using DependencyChecker.App;
using DependencyChecker.ExternalLib;
using DependencyChecker.SharedLib;

namespace DependencyChecker.Tests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void ValidateDependencies()
    {
        var result = ServiceCollectionValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithValidation(including => including
                .Controllers())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateDependencies_OtherEntryUsage()
    {
        var result = ServiceCollectionValidator
            .ForEntryAssembly(typeof(WeatherForecast).Assembly)
            .WithValidation(including => including
                .Controllers())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateDependencies_WithSomeAssumedServices()
    {
        var result = ServiceCollectionValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithValidation(including => including
                .Controllers())
            .AssumingExistenceOf(services => services
                .Including<IGeoService>())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateDependencies_With_RequiredServicesGetFromProvider()
    {
        var result = ServiceCollectionValidator
            .ForEntryAssembly<WeatherForecast>()
            .WithAdditional(assemblies => assemblies
                .Including(typeof(ServiceCollectionExtensions).Assembly))
            .WithValidation(including => including
                .Controllers()
                .GetRequiredServiceCalls())
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True, result.ToString());
    }

    [Test]
    public void PrintCallStack()
    {
        using var callsFinder = new MethodCallRoutesFinder(typeof(WeatherForecast).Assembly.Location);

        var methodToBeFound = typeof(IServiceProvider)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name == "GetService");

        var callRoutesToGetService = callsFinder.FindCallRoutesTo(methodToBeFound);

        foreach (var callStack in callRoutesToGetService)
        {
            Console.WriteLine(string.Join($"{Environment.NewLine}-> ",
                callStack.Select(x => $"{x.DeclaringType}.{x.Name}({string.Join(", ", x.Parameters.Select(p => $"{p.ParameterType} {p.Name}"))})")));
        }
    }
}
