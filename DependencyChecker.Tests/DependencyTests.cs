using System.Reflection;
using Dant.AspNetDependencyValidator;
using Dant.AspNetDependencyValidator.CallsFinding;
using DependencyChecker.App;
using DependencyChecker.ExternalLib;

namespace DependencyChecker.Tests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void ValidateDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<WeatherForecast>();
        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateDependencies_With_RequiredServicesGetFromProvider()
    {
        var result = AspNetDependenciesValidator.Validate(typeof(WeatherForecast).Assembly.Location, typeof(ServiceCollectionExtensions).Assembly.Location);

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateDependencies_With_RequiredServicesGetFromProvider2()
    {
        var result = ServiceCollectionValidator<WeatherForecast>.ForEntryAssembly()
            .And(typeof(ServiceCollectionExtensions).Assembly)
            .WithValidationsOf()
            .Controllers()
            .GetRequiredServiceTypes()
            .Build()
            .Run();

        Console.WriteLine(result);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void PrintCallStack()
    {
        using var callsFinder = new MethodCallsFinder(typeof(WeatherForecast).Assembly.Location);

        var methodToBeFound = typeof(IServiceProvider).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name == "GetService");

        var callRoutesToGetService = callsFinder.FindCallRoutesTo(methodToBeFound);

        foreach (var callStack in callRoutesToGetService)
        {
            Console.WriteLine(string.Join($"{Environment.NewLine}-> ",
                callStack.Select(x => $"{x.DeclaringType}.{x.Name}({string.Join(", ", x.Parameters.Select(p => $"{p.ParameterType} {p.Name}"))})")));
        }
    }
}
