using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IProviderSelectionEngine
{
    ValueTask<IProviderAdapter> SelectPrimaryAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        CancellationToken cancellationToken);

    ValueTask<IProviderAdapter?> SelectFallbackAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter> excludedAdapters,
        CancellationToken cancellationToken);
}
