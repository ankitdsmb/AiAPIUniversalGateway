using System.Globalization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Configuration;

namespace UniversalAPIGateway.Infrastructure.Services;

public sealed class ProviderIntelligenceEngine(
    IConnectionMultiplexer multiplexer,
    IQuotaService quotaService,
    IProviderHealthTracker providerHealthTracker,
    IOptions<ProviderIntelligenceOptions> options) : IProviderScoringService
{
    private readonly IDatabase database = multiplexer.GetDatabase();
    private readonly ProviderIntelligenceOptions scoringOptions = options.Value;

    public async ValueTask<double> ScoreAsync(Provider provider, ProviderCapability requiredCapability, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!provider.Supports(requiredCapability) || !provider.IsEnabled)
        {
            return double.NegativeInfinity;
        }

        var providerId = provider.Key.Value;
        var healthStatus = await providerHealthTracker.GetStatusAsync(providerId, cancellationToken);
        if (healthStatus == ProviderHealthStatus.Disabled)
        {
            return double.NegativeInfinity;
        }

        var healthKey = BuildHealthKey(providerId);
        var scoreKey = BuildScoreKey(providerId);

        var entries = await database.HashGetAllAsync(healthKey);
        var snapshot = ProviderHealthSnapshot.From(entries);

        var quota = await quotaService.GetQuotaAsync(providerId, cancellationToken);
        var successRate = snapshot.SuccessRate;
        var quotaRemaining = quota.DailyLimit <= 0 ? 0 : (double)quota.Remaining / quota.DailyLimit;
        var normalizedLatency = Math.Clamp(snapshot.AverageLatencyMilliseconds / scoringOptions.MaxLatencyMilliseconds, 0, 1);
        var normalizedFailures = Math.Clamp(snapshot.RecentFailures / scoringOptions.MaxRecentFailures, 0, 1);

        var score =
            (successRate * scoringOptions.SuccessRateWeight)
            + (quotaRemaining * scoringOptions.QuotaRemainingWeight)
            - (normalizedLatency * scoringOptions.LatencyWeight)
            - (normalizedFailures * scoringOptions.RecentFailuresWeight);

        if (healthStatus == ProviderHealthStatus.Degraded)
        {
            score -= scoringOptions.RecentFailuresWeight;
        }

        var tx = database.CreateTransaction();
        _ = tx.HashSetAsync(healthKey,
        [
            new HashEntry("quotaRemaining", quota.Remaining),
            new HashEntry("quotaLimit", quota.DailyLimit)
        ]);
        _ = tx.StringSetAsync(scoreKey, score.ToString(CultureInfo.InvariantCulture));
        await tx.ExecuteAsync();

        return score;
    }

    public async ValueTask RecordOutcomeAsync(RequestLog requestLog, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var providerId = requestLog.ProviderKey.Value;
        var key = BuildHealthKey(providerId);
        var tx = database.CreateTransaction();

        if (requestLog.Succeeded)
        {
            _ = tx.HashIncrementAsync(key, "successes", 1);
            _ = tx.HashIncrementAsync(key, "recentFailures", -1);
        }
        else
        {
            _ = tx.HashIncrementAsync(key, "failures", 1);
            _ = tx.HashIncrementAsync(key, "recentFailures", 1);
        }

        _ = tx.HashIncrementAsync(key, "totalRequests", 1);
        _ = tx.HashIncrementAsync(key, "totalLatencyMs", requestLog.Duration.TotalMilliseconds);
        await tx.ExecuteAsync();

        var currentRecentFailures = (double?)await database.HashGetAsync(key, "recentFailures") ?? 0;
        if (currentRecentFailures < 0)
        {
            await database.HashSetAsync(key, "recentFailures", 0);
        }
    }

    private static string BuildHealthKey(string providerId) => $"provider_health:{providerId}";

    private static string BuildScoreKey(string providerId) => $"provider_score:{providerId}";

    private sealed record ProviderHealthSnapshot(
        double Successes,
        double Failures,
        double TotalRequests,
        double TotalLatencyMilliseconds,
        double RecentFailures)
    {
        public double SuccessRate => TotalRequests <= 0 ? 1 : Successes / TotalRequests;

        public double AverageLatencyMilliseconds => TotalRequests <= 0 ? 0 : TotalLatencyMilliseconds / TotalRequests;

        public static ProviderHealthSnapshot From(HashEntry[] entries)
        {
            if (entries.Length == 0)
            {
                return new ProviderHealthSnapshot(0, 0, 0, 0, 0);
            }

            var values = entries.ToDictionary(static x => x.Name.ToString(), static x => (double)x.Value);

            values.TryGetValue("successes", out var successes);
            values.TryGetValue("failures", out var failures);
            values.TryGetValue("totalRequests", out var totalRequests);
            values.TryGetValue("totalLatencyMs", out var totalLatencyMs);
            values.TryGetValue("recentFailures", out var recentFailures);

            return new ProviderHealthSnapshot(successes, failures, totalRequests, totalLatencyMs, Math.Max(0, recentFailures));
        }
    }
}
