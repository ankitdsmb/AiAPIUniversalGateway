using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IProviderAdapter
{
    string ProviderKey { get; }

    Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken);
}
