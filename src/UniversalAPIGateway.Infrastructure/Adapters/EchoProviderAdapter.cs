using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class EchoProviderAdapter : IProviderAdapter
{
    public string ProviderKey => "echo";

    public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        var response = new GatewayResponse(ProviderKey, payload);
        return Task.FromResult(response);
    }
}
