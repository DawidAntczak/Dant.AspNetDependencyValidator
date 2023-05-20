using Dant.AspNetDependencyValidator;
using DependencyChecker.App;

namespace DependencyChecker.Tests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void ValidateDependencies()
    {
        var result = AspNetDependenciesValidator.Validate<WeatherForecast>(validateServiceCollection: true);
        Console.WriteLine(result.Message);
        Assert.That(result.IsValid, Is.True);
    }
}
