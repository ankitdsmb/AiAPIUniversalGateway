using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class DefaultProviderSelectionStrategy(IAdaptiveRoutingEngine adaptiveRoutingEngine) : IProviderSelectionStrategy
{
    private const string AutoProviderKey = "auto";

    public async ValueTask<IProviderAdapter> SelectPrimaryAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ProviderKey.Value.Equals(AutoProviderKey, StringComparison.OrdinalIgnoreCase))
        {
            var bestAdapter = await adaptiveRoutingEngine.SelectAdapterAsync(adapters, request, excludedAdapters: null, cancellationToken);
            return bestAdapter ?? throw new InvalidOperationException("No eligible provider is available for automatic selection.");
        }

        var adapter = adapters.FirstOrDefault(x =>
            x.Provider.Key.Value.Equals(request.ProviderKey.Value, StringComparison.OrdinalIgnoreCase)
            && x.Provider.IsEnabled);

        return adapter is null
            ? throw new InvalidOperationException($"Provider '{request.ProviderKey}' is not registered.")
            : adapter;
    }

    public ValueTask<IProviderAdapter?> SelectFallbackAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter> excludedAdapters,
        CancellationToken cancellationToken) =>
        adaptiveRoutingEngine.SelectAdapterAsync(adapters, request, excludedAdapters, cancellationToken);
}
