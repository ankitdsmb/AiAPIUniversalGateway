using Microsoft.Extensions.Configuration;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;

namespace UniversalAPIGateway.Application.Tests;

public sealed class ProviderRegistryServiceTests
{
    [Fact]
    public async Task RegisterAsync_WritesToPersistenceAndCache()
    {
        var persistence = new InMemoryPersistence();
        var cache = new InMemoryCache();
        var sut = CreateSut(persistence, cache);

        var entry = await sut.RegisterAsync(
            new ProviderRegistration("provider-a", "Provider A", "http://provider-a", new[] { "chat", "vision" }),
            CancellationToken.None);

        Assert.True(persistence.Entries.ContainsKey("provider-a"));
        Assert.Contains("provider-a", cache.Keys);
        Assert.True(entry.IsEnabled);
    }

    [Fact]
    public async Task HeartbeatAsync_ReturnsNull_WhenProviderMissing()
    {
        var sut = CreateSut(new InMemoryPersistence(), new InMemoryCache());

        var entry = await sut.HeartbeatAsync("missing", CancellationToken.None);

        Assert.Null(entry);
    }

    [Fact]
    public async Task HeartbeatAsync_DisablesStaleProviders()
    {
        var persistence = new InMemoryPersistence();
        var cache = new InMemoryCache();
        var sut = CreateSut(persistence, cache, heartbeatTimeoutSeconds: 10);

        await sut.RegisterAsync(new ProviderRegistration("fresh", "Fresh", "http://fresh", new[] { "chat" }), CancellationToken.None);

        persistence.Entries["stale"] = new ProviderRegistryEntry(
            "stale",
            "Stale",
            "http://stale",
            "chat",
            true,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddMinutes(-5));
        await cache.SetAsync(persistence.Entries["stale"], CancellationToken.None);

        await sut.HeartbeatAsync("fresh", CancellationToken.None);

        Assert.False(persistence.Entries["stale"].IsEnabled);
        Assert.DoesNotContain("stale", cache.Keys);
    }

    private static ProviderRegistryService CreateSut(
        InMemoryPersistence persistence,
        InMemoryCache cache,
        int heartbeatTimeoutSeconds = 90)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProviderRegistry:HeartbeatTimeoutSeconds"] = heartbeatTimeoutSeconds.ToString()
            })
            .Build();

        return new ProviderRegistryService(persistence, cache, config);
    }

    private sealed class InMemoryPersistence : IProviderRegistryPersistence
    {
        public Dictionary<string, ProviderRegistryEntry> Entries { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task UpsertAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken)
        {
            Entries[entry.ProviderKey] = entry;
            return Task.CompletedTask;
        }

        public Task<ProviderRegistryEntry?> GetByKeyAsync(string providerKey, CancellationToken cancellationToken)
        {
            Entries.TryGetValue(providerKey, out var entry);
            return Task.FromResult(entry);
        }

        public Task<IReadOnlyCollection<ProviderRegistryEntry>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult((IReadOnlyCollection<ProviderRegistryEntry>)Entries.Values.ToArray());

        public Task<bool> SetEnabledAsync(string providerKey, bool isEnabled, DateTimeOffset updatedAtUtc, CancellationToken cancellationToken)
        {
            if (!Entries.TryGetValue(providerKey, out var entry))
            {
                return Task.FromResult(false);
            }

            Entries[providerKey] = entry with { IsEnabled = isEnabled, UpdatedAtUtc = updatedAtUtc };
            return Task.FromResult(true);
        }

        public Task<IReadOnlyCollection<string>> DisableStaleAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken)
        {
            var disabled = new List<string>();
            foreach (var key in Entries.Keys.ToArray())
            {
                var entry = Entries[key];
                if (!entry.IsEnabled || entry.LastHeartbeatUtc >= staleBeforeUtc)
                {
                    continue;
                }

                Entries[key] = entry with { IsEnabled = false, UpdatedAtUtc = DateTimeOffset.UtcNow };
                disabled.Add(key);
            }

            return Task.FromResult((IReadOnlyCollection<string>)disabled);
        }
    }

    private sealed class InMemoryCache : IProviderRegistryCache
    {
        public HashSet<string> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task SetAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken)
        {
            Keys.Add(entry.ProviderKey);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string providerKey, CancellationToken cancellationToken)
        {
            Keys.Remove(providerKey);
            return Task.CompletedTask;
        }
    }
}
