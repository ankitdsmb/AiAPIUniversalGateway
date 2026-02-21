using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class DefaultProviderSelectionStrategy : IProviderSelectionStrategy
{
    public ValueTask<IProviderAdapter> SelectPrimaryAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var adapter = adapters.FirstOrDefault(x =>
            x.Provider.Key.Value.Equals(request.ProviderKey.Value, StringComparison.OrdinalIgnoreCase)
            && x.Provider.IsEnabled);

        return adapter is null
            ? ValueTask.FromException<IProviderAdapter>(new InvalidOperationException($"Provider '{request.ProviderKey}' is not registered."))
            : ValueTask.FromResult(adapter);
    }

    public ValueTask<IProviderAdapter?> SelectFallbackAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IProviderAdapter failedAdapter,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fallback = adapters.FirstOrDefault(x =>
            !ReferenceEquals(x, failedAdapter)
            && x.Provider.IsEnabled
            && x.Provider.Supports(ProviderCapability.TextGeneration));

        return ValueTask.FromResult(fallback);
    }
}
