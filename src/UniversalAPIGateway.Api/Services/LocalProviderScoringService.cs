using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Api.Services;

public sealed class LocalProviderScoringService : IProviderScoringService
{
    public ValueTask<double> ScoreAsync(Provider provider, ProviderCapability requiredCapability, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(1d);
    }

    public ValueTask RecordOutcomeAsync(RequestLog requestLog, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }
}
