using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IAdaptiveRoutingEngine
{
    ValueTask<IProviderAdapter?> SelectAdapterAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter>? excludedAdapters,
        CancellationToken cancellationToken);

    ValueTask RecordOutcomeAsync(
        GatewayRequest request,
        string providerId,
        bool succeeded,
        TimeSpan latency,
        string? responsePayload,
        int tokenUsage,
        CancellationToken cancellationToken);
}
