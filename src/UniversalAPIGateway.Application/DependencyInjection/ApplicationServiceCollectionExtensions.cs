using Microsoft.Extensions.DependencyInjection;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;

namespace UniversalAPIGateway.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProviderSelectionStrategy, DefaultProviderSelectionStrategy>();
        services.AddScoped<IProviderSelectionEngine, ProviderSelectionEngine>();
        services.AddScoped<IFallbackHandler, FallbackHandler>();
        services.AddScoped<IResponseNormalizer, ResponseNormalizer>();
        services.AddScoped<IOrchestratorService, OrchestratorService>();
        services.AddScoped<IGatewayService, GatewayService>();
        services.AddScoped<IProviderRegistryService, ProviderRegistryService>();
        return services;
    }
}
