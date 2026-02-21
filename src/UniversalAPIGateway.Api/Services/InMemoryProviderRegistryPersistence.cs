using System.Collections.Concurrent;
using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Api.Services;

public sealed class InMemoryProviderRegistryPersistence : IProviderRegistryPersistence
{
    private readonly ConcurrentDictionary<string, ProviderRegistryEntry> entries = new(StringComparer.OrdinalIgnoreCase);

    public Task UpsertAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entries[entry.ProviderKey] = entry;
        return Task.CompletedTask;
    }

    public Task<ProviderRegistryEntry?> GetByKeyAsync(string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entries.TryGetValue(providerKey, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyCollection<ProviderRegistryEntry>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<ProviderRegistryEntry>>(entries.Values.ToArray());
    }

    public Task<bool> SetEnabledAsync(string providerKey, bool isEnabled, DateTimeOffset updatedAtUtc, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!entries.TryGetValue(providerKey, out var current))
        {
            return Task.FromResult(false);
        }

        entries[providerKey] = current with
        {
            IsEnabled = isEnabled,
            UpdatedAtUtc = updatedAtUtc
        };

        return Task.FromResult(true);
    }

    public Task<IReadOnlyCollection<string>> DisableStaleAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var disabled = new List<string>();

        foreach (var (providerKey, entry) in entries)
        {
            if (entry.IsEnabled && entry.LastHeartbeatUtc < staleBeforeUtc)
            {
                entries[providerKey] = entry with { IsEnabled = false, UpdatedAtUtc = DateTimeOffset.UtcNow };
                disabled.Add(providerKey);
            }
        }

        return Task.FromResult<IReadOnlyCollection<string>>(disabled);
    }
}
