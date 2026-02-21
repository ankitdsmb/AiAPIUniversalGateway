using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Domain.Ports;

public interface IProviderAdapter
{
    Provider Provider { get; }

    Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken);
}
