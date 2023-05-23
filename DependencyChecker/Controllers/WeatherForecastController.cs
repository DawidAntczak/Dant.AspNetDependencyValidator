using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

namespace DependencyChecker.App.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDiagnostics _diagnostics;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IDiagnostics diagnostics)
    {
        _logger = logger;
        _diagnostics = diagnostics;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get([FromServices] IWeatherService weatherService)
    {
        _diagnostics.Log("Executing Weather Forecast Get endpoint.");
        return weatherService.Get();
    }
}


public interface IWeatherService
{
    IEnumerable<WeatherForecast> Get();
}

public class WeatherService : IWeatherService
{
    private readonly ISystemClock _systemClock;
    private readonly ILogger<WeatherService> _logger;
    private readonly IServiceProvider serviceProvider;

    public WeatherService(ISystemClock systemClock, ILogger<WeatherService> logger, IServiceProvider provider)
    {
        //provider.GetRequiredService<ILogger<WeatherService>>();
        var dupa = () => provider.GetRequiredService<ILogger<WeatherService>>();
        dupa();
        provider.GetService(typeof(ISystemClock));
        _systemClock = systemClock;
        _logger = logger;
        serviceProvider = provider;
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public IEnumerable<WeatherForecast> Get()
    {
        serviceProvider.GetService<ISystemClock>();

        Console.WriteLine(_systemClock.UtcNow);

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}

public interface IDiagnostics
{
    void Log(string message);
}

public class ConsoleDiagnostics : IDiagnostics
{
    private readonly ILogger<ConsoleDiagnostics> _logger;

    public ConsoleDiagnostics(ILogger<ConsoleDiagnostics> logger)
    {
        _logger = logger;
    }

    public void Log(string message)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace(message);
    }
}