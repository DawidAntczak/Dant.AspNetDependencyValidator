using DependencyChecker.SharedLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

namespace DependencyChecker.App.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDiagnostics _diagnostics;
    private readonly IGeoService _geoService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IDiagnostics diagnostics, IGeoService geoService)
    {
        _logger = logger;
        _diagnostics = diagnostics;
        _geoService = geoService;
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
    private readonly IServiceProvider _serviceProvider;

    public WeatherService(ISystemClock systemClock, ILogger<WeatherService> logger, IServiceProvider provider/*, IEndpointAddressScheme<IEndpointFilter> eee*/)
    {
        var lambda = () => provider.GetRequiredService<ILogger<WeatherService>>();
        lambda();
        provider.GetService(typeof(ISystemClock));
        _systemClock = systemClock;
        _logger = logger;
        _serviceProvider = provider;
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public IEnumerable<WeatherForecast> Get()
    {
        _serviceProvider.GetService<ISystemClock>();

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