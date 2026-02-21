using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;

namespace UniversalAPIGateway.Infrastructure.Services;

public interface IProviderHealthTracker
{
    Task MarkTemporaryUnavailableAsync(string providerKey, string reason, TimeSpan cooldown, CancellationToken cancellationToken);

    Task MarkHealthyAsync(string providerKey, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> DisableUnhealthyProvidersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> RecoverProvidersAsync(CancellationToken cancellationToken);

    Task<ProviderHealthStatus> GetStatusAsync(string providerKey, CancellationToken cancellationToken);
}

public sealed class ProviderHealthTracker(
    IConnectionMultiplexer multiplexer,
    IProviderRegistryPersistence registryPersistence,
    IProviderRegistryCache registryCache,
    IOptions<ProviderHealthLifecycleOptions> options) : IProviderHealthTracker
{
    private readonly IDatabase database = multiplexer.GetDatabase();
    private readonly ProviderHealthLifecycleOptions lifecycleOptions = options.Value;

    public async Task MarkTemporaryUnavailableAsync(string providerKey, string reason, TimeSpan cooldown, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildHealthKey(providerKey);
        var status = reason switch
        {
            "quota_exceeded" => ProviderHealthStatus.QuotaExceeded,
            "rate_limited" => ProviderHealthStatus.RateLimited,
            _ => ProviderHealthStatus.Degraded
        };

        _ = await database.HashIncrementAsync(key, "recentFailures", 1);
        await database.HashSetAsync(key,
        [
            new HashEntry("status", status.ToString()),
            new HashEntry("reason", reason),
            new HashEntry("lastFailureUtc", DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            new HashEntry("cooldownUntilUtc", DateTimeOffset.UtcNow.Add(cooldown).ToUnixTimeSeconds())
        ]);
    }

    public async Task MarkHealthyAsync(string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildHealthKey(providerKey);
        _ = await database.HashIncrementAsync(key, "recentSuccesses", 1);
        await database.HashSetAsync(key,
        [
            new HashEntry("status", ProviderHealthStatus.Healthy.ToString()),
            new HashEntry("reason", string.Empty)
        ]);

        var recentFailures = (double?)await database.HashGetAsync(key, "recentFailures") ?? 0;
        if (recentFailures > 0)
        {
            _ = await database.HashIncrementAsync(key, "recentFailures", -1);
        }
    }

    public async Task<IReadOnlyCollection<string>> DisableUnhealthyProvidersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var providers = await registryPersistence.GetAllAsync(cancellationToken);
        var disabled = new List<string>();

        foreach (var provider in providers.Where(static x => x.IsEnabled))
        {
            var status = await GetStatusAsync(provider.ProviderKey, cancellationToken);
            var failures = (int?)await database.HashGetAsync(BuildHealthKey(provider.ProviderKey), "recentFailures") ?? 0;

            if (status is ProviderHealthStatus.QuotaExceeded or ProviderHealthStatus.RateLimited || failures >= lifecycleOptions.FailureThreshold)
            {
                var updated = await registryPersistence.SetEnabledAsync(provider.ProviderKey, false, now, cancellationToken);
                if (!updated)
                {
                    continue;
                }

                await registryCache.RemoveAsync(provider.ProviderKey, cancellationToken);
                await database.HashSetAsync(BuildHealthKey(provider.ProviderKey),
                [
                    new HashEntry("status", ProviderHealthStatus.Disabled.ToString()),
                    new HashEntry("disabledUntilUtc", now.AddSeconds(lifecycleOptions.CooldownSeconds).ToUnixTimeSeconds())
                ]);
                disabled.Add(provider.ProviderKey);
            }
        }

        return disabled;
    }

    public async Task<IReadOnlyCollection<string>> RecoverProvidersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var recovered = new List<string>();
        var providers = await registryPersistence.GetAllAsync(cancellationToken);

        foreach (var provider in providers.Where(static x => !x.IsEnabled))
        {
            var key = BuildHealthKey(provider.ProviderKey);
            var status = await GetStatusAsync(provider.ProviderKey, cancellationToken);
            if (status != ProviderHealthStatus.Disabled)
            {
                continue;
            }

            var disabledUntil = (long?)await database.HashGetAsync(key, "disabledUntilUtc") ?? 0;
            var recentSuccesses = (int?)await database.HashGetAsync(key, "recentSuccesses") ?? 0;
            var canRecover = now >= disabledUntil && recentSuccesses >= lifecycleOptions.RecoverySuccessThreshold;
            if (!canRecover)
            {
                continue;
            }

            var updated = await registryPersistence.SetEnabledAsync(provider.ProviderKey, true, DateTimeOffset.UtcNow, cancellationToken);
            if (!updated)
            {
                continue;
            }

            var refreshedProvider = provider with { IsEnabled = true, UpdatedAtUtc = DateTimeOffset.UtcNow };
            await registryCache.SetAsync(refreshedProvider, cancellationToken);
            await database.HashSetAsync(key,
            [
                new HashEntry("status", ProviderHealthStatus.Healthy.ToString()),
                new HashEntry("recentFailures", 0)
            ]);

            recovered.Add(provider.ProviderKey);
        }

        return recovered;
    }

    public async Task<ProviderHealthStatus> GetStatusAsync(string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value = await database.HashGetAsync(BuildHealthKey(providerKey), "status");
        if (!value.HasValue)
        {
            return ProviderHealthStatus.Healthy;
        }

        return Enum.TryParse<ProviderHealthStatus>(value.ToString(), ignoreCase: true, out var status)
            ? status
            : ProviderHealthStatus.Healthy;
    }

    private static string BuildHealthKey(string providerKey) => $"provider_health:{providerKey}";
}
