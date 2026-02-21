using Microsoft.Extensions.DependencyInjection;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Api.Services;

public static class ApiRuntimeDefaultsServiceCollectionExtensions
{
    public static IServiceCollection AddApiRuntimeDefaults(this IServiceCollection services)
    {
        services.AddSingleton<IProviderAdapter, LocalEchoProviderAdapter>();
        services.AddSingleton<IProviderScoringService, LocalProviderScoringService>();
        services.AddSingleton<IProviderRegistryPersistence, InMemoryProviderRegistryPersistence>();
        services.AddSingleton<IProviderRegistryCache, NoOpProviderRegistryCache>();
        return services;
    }
}
