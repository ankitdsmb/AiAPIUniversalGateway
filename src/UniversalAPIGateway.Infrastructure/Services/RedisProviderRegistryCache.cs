using System.Text.Json;
using StackExchange.Redis;
using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Infrastructure.Services;

public sealed class RedisProviderRegistryCache(IConnectionMultiplexer multiplexer) : IProviderRegistryCache
{
    private readonly IDatabase database = multiplexer.GetDatabase();

    public Task SetAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = BuildKey(entry.ProviderKey);
        var value = JsonSerializer.Serialize(entry);
        return database.StringSetAsync(key, value);
    }

    public Task RemoveAsync(string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return database.KeyDeleteAsync(BuildKey(providerKey));
    }

    private static string BuildKey(string providerKey) => $"provider_registry:{providerKey}";
}
