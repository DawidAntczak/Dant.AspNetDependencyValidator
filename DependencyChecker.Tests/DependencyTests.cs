using System.Reflection.Emit;
using Dant.AspNetDependencyValidator;
using Dant.AspNetDependencyValidator.SourceAnalysis;
using DependencyChecker.App;

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
    public void T()
    {
        foreach (var callStack in CallsFinder.FindCallsToGetService<WeatherForecast>())
        {
            Console.WriteLine(string.Join($"{Environment.NewLine} -> ",
                callStack.Select(x => $"{x.DeclaringType}.{x.Name}({string.Join(", ", x.Parameters.Select(p => $"{p.ParameterType} {p.Name}"))})")));
        }
    }
}
