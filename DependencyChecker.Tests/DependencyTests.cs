using System.Reflection;
using Dant.AspNetDependencyValidator;
using Dant.AspNetDependencyValidator.CodeAnalysis;
using DependencyChecker.App;
using DependencyChecker.App.Controllers;

namespace DependencyChecker.Tests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void ValidateDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<WeatherForecast>();
        Console.WriteLine(result.Message);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateDependencies_With_RequiredServicesGetFromProvider()
    {
        var genericMethod = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.ContainsGenericParameters && m.Name == "GetRequiredService");

        using var genericTypesFinder = new GenericTypesFinder(typeof(WeatherForecast).Assembly.Location);

        var usedGenericTypes = genericTypesFinder.FindUsedByMethodGenericTypes(genericMethod);

        var result = AspNetDependenciesValidator.Validate<WeatherForecast>(usedGenericTypes.Select(t => t.UsedType));
        Console.WriteLine(result.Message);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void T1()
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
