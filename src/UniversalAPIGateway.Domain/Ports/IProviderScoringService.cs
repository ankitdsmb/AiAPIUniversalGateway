using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IProviderScoringService
{
    ValueTask<double> ScoreAsync(Provider provider, ProviderCapability requiredCapability, CancellationToken cancellationToken);

    ValueTask RecordOutcomeAsync(RequestLog requestLog, CancellationToken cancellationToken);
}
