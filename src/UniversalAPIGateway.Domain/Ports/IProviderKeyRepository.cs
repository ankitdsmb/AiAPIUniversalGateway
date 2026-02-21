using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IProviderKeyRepository
{
    ValueTask<IReadOnlyCollection<ProviderKey>> GetAllEnabledAsync(CancellationToken cancellationToken);
}
