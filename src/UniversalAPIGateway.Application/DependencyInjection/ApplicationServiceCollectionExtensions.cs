using Microsoft.Extensions.DependencyInjection;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;

namespace UniversalAPIGateway.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGatewayService, GatewayService>();
        return services;
    }
}
