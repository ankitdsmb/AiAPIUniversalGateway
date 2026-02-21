using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Strategies;

public sealed class ProviderSelectionStrategy : IProviderSelector
{
    public ValueTask<IProviderAdapter> SelectAsync(
        IEnumerable<IProviderAdapter> adapters,
        ProviderKey preferredProvider,
        ProviderCapability requiredCapability,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var adapter = adapters.FirstOrDefault(x =>
            x.Provider.Key.Value.Equals(preferredProvider.Value, StringComparison.OrdinalIgnoreCase)
            && x.Provider.IsEnabled
            && x.Provider.Supports(requiredCapability));

        if (adapter is null)
        {
            throw new InvalidOperationException($"Provider '{preferredProvider}' is not registered for capability '{requiredCapability}'.");
        }

        return ValueTask.FromResult(adapter);
    }
}
