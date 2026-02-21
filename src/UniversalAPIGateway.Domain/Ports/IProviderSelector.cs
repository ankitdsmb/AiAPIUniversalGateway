using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IProviderSelector
{
    ValueTask<IProviderAdapter> SelectAsync(
        IEnumerable<IProviderAdapter> adapters,
        ProviderKey preferredProvider,
        ProviderCapability requiredCapability,
        CancellationToken cancellationToken);
}
