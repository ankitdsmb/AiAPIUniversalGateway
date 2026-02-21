using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Providers;
using UniversalAPIGateway.Infrastructure.Repositories;
using UniversalAPIGateway.Infrastructure.Services;
using UniversalAPIGateway.Infrastructure.Strategies;

namespace UniversalAPIGateway.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PortkeyOptions>(configuration.GetSection(PortkeyOptions.SectionName));
        services.Configure<ProviderEndpointOptions>(OpenRouterAdapter.OptionsName, configuration.GetSection("Providers:OpenRouter"));
        services.Configure<ProviderEndpointOptions>(HuggingFaceAdapter.OptionsName, configuration.GetSection("Providers:HuggingFace"));
        services.Configure<ProviderEndpointOptions>(TogetherAIAdapter.OptionsName, configuration.GetSection("Providers:TogetherAI"));
        services.Configure<ProviderEndpointOptions>(GroqAdapter.OptionsName, configuration.GetSection("Providers:Groq"));
        services.Configure<ProviderEndpointOptions>(ReplicateAdapter.OptionsName, configuration.GetSection("Providers:Replicate"));
        services.Configure<ProviderEndpointOptions>(MistralAdapter.OptionsName, configuration.GetSection("Providers:Mistral"));
        services.Configure<ProviderEndpointOptions>(AssemblyAIAdapter.OptionsName, configuration.GetSection("Providers:AssemblyAI"));
        services.Configure<ProviderEndpointOptions>(FireworksAdapter.OptionsName, configuration.GetSection("Providers:Fireworks"));
        services.Configure<ProviderEndpointOptions>(CohereAdapter.OptionsName, configuration.GetSection("Providers:Cohere"));
        services.Configure<ProviderEndpointOptions>(GoogleAIStudioAdapter.OptionsName, configuration.GetSection("Providers:GoogleAIStudio"));

        services.AddSingleton<IProviderResiliencePolicies, ProviderResiliencePolicies>();
        services.AddHttpClient<PortkeyAdapter>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PortkeyOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        });
        services.AddHttpClient<OpenRouterAdapter>((provider, client) =>
            ConfigureClient(client, provider, OpenRouterAdapter.OptionsName));
        services.AddHttpClient<HuggingFaceAdapter>((provider, client) =>
            ConfigureClient(client, provider, HuggingFaceAdapter.OptionsName));
        services.AddHttpClient<TogetherAIAdapter>((provider, client) =>
            ConfigureClient(client, provider, TogetherAIAdapter.OptionsName));
        services.AddHttpClient<GroqAdapter>((provider, client) =>
            ConfigureClient(client, provider, GroqAdapter.OptionsName));
        services.AddHttpClient<ReplicateAdapter>((provider, client) =>
            ConfigureClient(client, provider, ReplicateAdapter.OptionsName));
        services.AddHttpClient<MistralAdapter>((provider, client) =>
            ConfigureClient(client, provider, MistralAdapter.OptionsName));
        services.AddHttpClient<AssemblyAIAdapter>((provider, client) =>
            ConfigureClient(client, provider, AssemblyAIAdapter.OptionsName));
        services.AddHttpClient<FireworksAdapter>((provider, client) =>
            ConfigureClient(client, provider, FireworksAdapter.OptionsName));
        services.AddHttpClient<CohereAdapter>((provider, client) =>
            ConfigureClient(client, provider, CohereAdapter.OptionsName));
        services.AddHttpClient<GoogleAIStudioAdapter>((provider, client) =>
            ConfigureClient(client, provider, GoogleAIStudioAdapter.OptionsName));

        services.AddSingleton<IProviderAdapter, EchoProviderAdapter>();
        services.AddSingleton<IProviderAdapter, ReverseProviderAdapter>();
        services.AddSingleton<IProviderAdapter, MockProviderAdapter>();
        services.AddSingleton<IProviderHealthTracker, ProviderHealthTracker>();
        services.AddTransient<IProviderAdapter>(provider => provider.GetRequiredService<PortkeyAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<OpenRouterAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<HuggingFaceAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<TogetherAIAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<GroqAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<ReplicateAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<MistralAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<AssemblyAIAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<FireworksAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<CohereAdapter>());
        services.AddScoped<IProviderAdapter>(provider => provider.GetRequiredService<GoogleAIStudioAdapter>());

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

    private static void ConfigureClient(HttpClient client, IServiceProvider provider, string name)
    {
        var optionsMonitor = provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<ProviderEndpointOptions>>();
        var options = optionsMonitor.Get(name);

        if (!string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        }
    }
}
