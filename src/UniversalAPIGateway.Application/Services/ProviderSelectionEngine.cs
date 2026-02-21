using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class ProviderSelectionEngine : IProviderSelectionEngine
{
    private readonly IProviderSelectionStrategy strategy;

    public ProviderSelectionEngine(IProviderSelectionStrategy strategy)
    {
        this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

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
