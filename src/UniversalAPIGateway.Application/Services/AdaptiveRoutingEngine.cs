using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class AdaptiveRoutingEngine(
    ITaskClassifier taskClassifier,
    IProviderPerformanceStore performanceStore,
    IRandomSource randomSource) : IAdaptiveRoutingEngine
{
    private const double ExplorationRate = 0.10d;

    public async ValueTask<IProviderAdapter?> SelectAdapterAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter>? excludedAdapters,
        CancellationToken cancellationToken)
    {
        var taskType = taskClassifier.Classify(request);
        var requiredCapability = ResolveRequiredCapability(taskType);

        var candidates = new List<(IProviderAdapter Adapter, double Score)>();
        foreach (var adapter in adapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!adapter.Provider.IsEnabled)
            {
                continue;
            }

            if (excludedAdapters is not null && excludedAdapters.Contains(adapter))
            {
                continue;
            }

            if (requiredCapability != ProviderCapability.None && !adapter.Provider.Supports(requiredCapability))
            {
                continue;
            }

            var performance = await performanceStore.GetAsync(adapter.Provider.Key.Value, taskType, cancellationToken);
            candidates.Add((adapter, Score(performance)));
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Count > 1 && randomSource.NextDouble() < ExplorationRate)
        {
            var randomIndex = randomSource.NextInt(candidates.Count);
            return candidates[randomIndex].Adapter;
        }

        return candidates
            .OrderByDescending(static x => x.Score)
            .First()
            .Adapter;
    }

    public async ValueTask RecordOutcomeAsync(
        GatewayRequest request,
        string providerId,
        bool succeeded,
        TimeSpan latency,
        string? responsePayload,
        CancellationToken cancellationToken)
    {
        var taskType = taskClassifier.Classify(request);
        var qualityScore = ComputeQualityScore(succeeded, responsePayload);

        await performanceStore.UpdateOutcomeAsync(providerId, taskType, succeeded, latency, qualityScore, cancellationToken);
    }

    private static double Score(ProviderPerformance performance)
    {
        var latencyScore = 1d / (1d + (performance.Latency.TotalMilliseconds / 1_000d));
        return (performance.SuccessRate * 0.5d)
               + (performance.QualityScore * 0.35d)
               + (latencyScore * 0.15d);
    }

    private static double ComputeQualityScore(bool succeeded, string? responsePayload)
    {
        if (!succeeded)
        {
            return 0d;
        }

        if (string.IsNullOrWhiteSpace(responsePayload))
        {
            return 0.4d;
        }

        return Math.Clamp(responsePayload.Length / 200d, 0.4d, 1d);
    }

    private static ProviderCapability ResolveRequiredCapability(TaskType taskType) => taskType switch
    {
        TaskType.AudioTranscription => ProviderCapability.SpeechToText,
        _ => ProviderCapability.TextGeneration
    };
}
