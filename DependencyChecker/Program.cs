using DependencyChecker.App.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddLogging(options => options.ClearProviders());
builder.Services.AddLogging(options => options.AddDebug());
builder.Services.AddLogging(options => options.AddSimpleConsole());

builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddScoped<IWeatherService>(provider => new WeatherService(
    provider.GetRequiredService<ISystemClock>(), provider.GetRequiredService<ILogger<WeatherService>>(), provider.GetRequiredService<IServiceProvider>(), provider.GetRequiredService<IEndpointAddressScheme<IEndpointFilter>>()));
builder.Services.AddSingleton<IDiagnostics, ConsoleDiagnostics>();


var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
