using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Repositories;
using UniversalAPIGateway.Infrastructure.Services;
using UniversalAPIGateway.Infrastructure.Strategies;

namespace UniversalAPIGateway.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PortkeyOptions>(configuration.GetSection(PortkeyOptions.SectionName));

        services.AddSingleton<IProviderResiliencePolicies, ProviderResiliencePolicies>();
        services.AddHttpClient<PortkeyAdapter>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PortkeyOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        });

        services.AddSingleton<IProviderAdapter, EchoProviderAdapter>();
        services.AddSingleton<IProviderAdapter, ReverseProviderAdapter>();
        services.AddSingleton<IProviderAdapter, MockProviderAdapter>();
        services.AddTransient<IProviderAdapter>(provider => provider.GetRequiredService<PortkeyAdapter>());

        services.AddSingleton<IProviderSelector, ProviderSelectionStrategy>();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IQuotaService, RedisQuotaService>();

        var postgresConnectionString = configuration.GetConnectionString("PostgreSql")
            ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=universal_gateway";
        services.AddSingleton<IRequestLogRepository>(_ => new PostgreSqlRequestLogRepository(postgresConnectionString));
        services.AddSingleton<IProviderKeyRepository>(_ => new PostgreSqlProviderKeyRepository(postgresConnectionString));

        return services;
    }
}
