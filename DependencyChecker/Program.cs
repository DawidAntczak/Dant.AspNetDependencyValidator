using DependencyChecker.App;
using DependencyChecker.App.Controllers;
using DependencyChecker.App.Controllers.Filters;
using DependencyChecker.App.Middlewares;
using DependencyChecker.App.Pages;
using DependencyChecker.App.Pages.Filters;
using DependencyChecker.ExternalLib;
using DependencyChecker.SharedLib;
using Microsoft.Extensions.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddLogging(options => options.ClearProviders());
builder.Services.AddLogging(options => options.AddDebug());
builder.Services.AddLogging(options => options.AddSimpleConsole());

builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddScoped<IWeatherService>(provider => new WeatherService(
    provider.GetRequiredService<ISystemClock>(), provider.GetRequiredService<ILogger<WeatherService>>(), provider.GetRequiredService<IServiceProvider>()/*, provider.GetRequiredService<IEndpointAddressScheme<IEndpointFilter>>()*/));
builder.Services.AddSingleton<IDiagnostics, ConsoleDiagnostics>();
builder.Services.AddSingleton<LocationService>();

builder.Services.AddSingleton<MiddlewaresDependency>();

builder.Services.AddSingleton<ActionFiltersDependency>();

builder.Services.AddSingleton<PagesDependency>();
builder.Services.AddSingleton<PageFiltersDependency>();


builder.Services.RegisterExternalLibDependencies();

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<RequestCultureMiddleware>();

app.MapControllers();

app.MapRazorPages();

app.Run();
