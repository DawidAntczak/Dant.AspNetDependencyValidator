using DependencyChecker.SharedLib;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyChecker.ExternalLib
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterExternalLibDependencies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IGeoService>(sp => new GeoService(sp.GetRequiredService<LocationService>()));
        }
    }
}
