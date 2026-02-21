using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

internal sealed class TestAdaptiveRoutingEngine : IAdaptiveRoutingEngine
{
    public ValueTask<IProviderAdapter?> SelectAdapterAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter>? excludedAdapters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var selected = adapters.FirstOrDefault(x => excludedAdapters is null || !excludedAdapters.Contains(x));
        return ValueTask.FromResult(selected);
    }

    public ValueTask RecordOutcomeAsync(
        GatewayRequest request,
        string providerId,
        bool succeeded,
        TimeSpan latency,
        string? responsePayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }
}
