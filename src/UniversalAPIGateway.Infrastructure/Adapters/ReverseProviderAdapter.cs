using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class ReverseProviderAdapter : IProviderAdapter
{
    public string ProviderKey => "reverse";

    public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        var reversed = new string(payload.Reverse().ToArray());
        var response = new GatewayResponse(ProviderKey, reversed);
        return Task.FromResult(response);
    }
}
