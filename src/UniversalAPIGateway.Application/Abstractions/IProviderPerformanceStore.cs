using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IProviderPerformanceStore
{
    ValueTask<ProviderPerformance> GetAsync(string providerId, TaskType taskType, CancellationToken cancellationToken);

    ValueTask UpdateOutcomeAsync(
        string providerId,
        TaskType taskType,
        bool succeeded,
        TimeSpan latency,
        double qualityScore,
        CancellationToken cancellationToken);
}
