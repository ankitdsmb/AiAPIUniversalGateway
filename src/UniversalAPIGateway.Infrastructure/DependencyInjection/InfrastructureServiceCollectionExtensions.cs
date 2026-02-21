using Microsoft.Extensions.DependencyInjection;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Strategies;

namespace UniversalAPIGateway.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProviderAdapter, EchoProviderAdapter>();
        services.AddSingleton<IProviderAdapter, ReverseProviderAdapter>();
        services.AddSingleton<IProviderSelector, ProviderSelectionStrategy>();
        return services;
    }
}
