using Microsoft.Extensions.Configuration;
using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Application.Services;

public sealed class ProviderRegistryService(
    IProviderRegistryPersistence persistence,
    IProviderRegistryCache cache,
    IConfiguration configuration) : IProviderRegistryService
{
    private readonly TimeSpan heartbeatTimeout = TimeSpan.FromSeconds(
        Math.Max(5, configuration.GetValue<int?>("ProviderRegistry:HeartbeatTimeoutSeconds") ?? 90));

    public async Task<ProviderRegistryEntry> RegisterAsync(ProviderRegistration registration, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.ProviderKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.DisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.Endpoint);

        var normalizedKey = registration.ProviderKey.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;
        var entry = new ProviderRegistryEntry(
            normalizedKey,
            registration.DisplayName.Trim(),
            registration.Endpoint.Trim(),
            string.Join(',', registration.Capabilities.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase)),
            IsEnabled: true,
            LastHeartbeatUtc: now,
            UpdatedAtUtc: now);

        await persistence.UpsertAsync(entry, cancellationToken);
        await cache.SetAsync(entry, cancellationToken);
        await DisableStaleProvidersAsync(now, cancellationToken);

        return entry;
    }

    public async Task<ProviderRegistryEntry?> HeartbeatAsync(string providerKey, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);

        var normalizedKey = providerKey.Trim().ToLowerInvariant();
        var existing = await persistence.GetByKeyAsync(normalizedKey, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var updated = existing with
        {
            IsEnabled = true,
            LastHeartbeatUtc = now,
            UpdatedAtUtc = now
        };

        await persistence.UpsertAsync(updated, cancellationToken);
        await cache.SetAsync(updated, cancellationToken);
        await DisableStaleProvidersAsync(now, cancellationToken);

        return updated;
    }

    private async Task DisableStaleProvidersAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var disabledKeys = await persistence.DisableStaleAsync(now.Subtract(heartbeatTimeout), cancellationToken);
        foreach (var providerKey in disabledKeys)
        {
            await cache.RemoveAsync(providerKey, cancellationToken);
        }
    }
}
