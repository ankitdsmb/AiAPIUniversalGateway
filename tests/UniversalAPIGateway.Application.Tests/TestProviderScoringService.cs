using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

internal sealed class TestProviderScoringService : IProviderScoringService
{
    private readonly Dictionary<string, double> scores;

    public TestProviderScoringService(Dictionary<string, double>? scores = null)
    {
        this.scores = scores ?? new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    }

    public ValueTask<double> ScoreAsync(Provider provider, ProviderCapability requiredCapability, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(scores.TryGetValue(provider.Key.Value, out var score) ? score : 0d);
    }

    public ValueTask RecordOutcomeAsync(RequestLog requestLog, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }
}
