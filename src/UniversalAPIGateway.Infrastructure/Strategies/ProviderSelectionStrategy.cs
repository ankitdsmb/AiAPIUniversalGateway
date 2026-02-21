using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Strategies;

public sealed class ProviderSelectionStrategy(IEnumerable<IProviderAdapter> adapters) : IProviderSelectionStrategy
{
    private readonly IReadOnlyDictionary<string, IProviderAdapter> _adapters = adapters
        .ToDictionary(x => x.ProviderKey, StringComparer.OrdinalIgnoreCase);

    public IProviderAdapter Resolve(string providerKey)
    {
        if (_adapters.TryGetValue(providerKey, out var adapter))
        {
            return adapter;
        }

        throw new InvalidOperationException($"Provider '{providerKey}' is not registered.");
    }
}
