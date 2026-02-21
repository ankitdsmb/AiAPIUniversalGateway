using System.Collections.Concurrent;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Services;

public sealed class InMemoryProviderPerformanceStore : IProviderPerformanceStore
{
    private const double EmaFactor = 0.2d;
    private readonly ConcurrentDictionary<string, ProviderPerformance> performances = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<ProviderPerformance> GetAsync(string providerId, TaskType taskType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(providerId, taskType);
        var performance = performances.GetOrAdd(key, _ => ProviderPerformance.CreateDefault(providerId, taskType));
        return ValueTask.FromResult(performance);
    }

    public ValueTask UpdateOutcomeAsync(
        string providerId,
        TaskType taskType,
        bool succeeded,
        TimeSpan latency,
        double qualityScore,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(providerId, taskType);
        performances.AddOrUpdate(
            key,
            _ => CreateFromOutcome(providerId, taskType, succeeded, latency, qualityScore),
            (_, current) => UpdatePerformance(current, succeeded, latency, qualityScore));

        return ValueTask.CompletedTask;
    }

    private static ProviderPerformance CreateFromOutcome(string providerId, TaskType taskType, bool succeeded, TimeSpan latency, double qualityScore) =>
        new(providerId, taskType, succeeded ? 1d : 0d, latency, Math.Clamp(qualityScore, 0d, 1d), 1);

    private static ProviderPerformance UpdatePerformance(ProviderPerformance current, bool succeeded, TimeSpan latency, double qualityScore)
    {
        var successValue = succeeded ? 1d : 0d;
        var normalizedQuality = Math.Clamp(qualityScore, 0d, 1d);

        var successRate = ApplyEma(current.SuccessRate, successValue);
        var latencyMs = ApplyEma(current.Latency.TotalMilliseconds, latency.TotalMilliseconds);
        var quality = ApplyEma(current.QualityScore, normalizedQuality);

        return current with
        {
            SuccessRate = successRate,
            Latency = TimeSpan.FromMilliseconds(Math.Max(1, latencyMs)),
            QualityScore = quality,
            SampleSize = current.SampleSize + 1
        };
    }

    private static double ApplyEma(double baseline, double latest) => ((1 - EmaFactor) * baseline) + (EmaFactor * latest);

    private static string BuildKey(string providerId, TaskType taskType) => $"{providerId}:{taskType}";
}
