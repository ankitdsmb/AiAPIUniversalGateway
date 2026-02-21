using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class ProviderSelectionEngine(IProviderSelectionStrategy strategy) : IProviderSelectionEngine
{
    public ValueTask<IProviderAdapter> SelectPrimaryAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        CancellationToken cancellationToken) =>
        strategy.SelectPrimaryAsync(adapters, request, cancellationToken);

    public ValueTask<IProviderAdapter?> SelectFallbackAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter> excludedAdapters,
        CancellationToken cancellationToken) =>
        strategy.SelectFallbackAsync(adapters, request, excludedAdapters, cancellationToken);
}
