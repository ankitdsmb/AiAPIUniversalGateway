using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IProviderSelectionStrategy
{
    IProviderAdapter Resolve(string providerKey);
}
