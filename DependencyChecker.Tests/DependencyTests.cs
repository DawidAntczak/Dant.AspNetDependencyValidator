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
        foreach (var invocation in CallsFinder.FindCalls(@"C:\Repos\Dant.AspNetDependencyValidator\DependencyChecker\bin\Debug\net7.0\DependencyChecker.App.dll"))
        {
            //typeof(WeatherForecast).Assembly
            Console.WriteLine(invocation);
        }
    }
}
