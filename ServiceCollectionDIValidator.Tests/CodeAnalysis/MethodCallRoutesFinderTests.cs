using DependencyChecker.App;
using ServiceCollectionDIValidator.CodeAnalysis.CallRoutes;
using System.Reflection;

namespace ServiceCollectionDIValidator.Tests.CodeAnalysis;

public class MethodCallRoutesFinderTests
{
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