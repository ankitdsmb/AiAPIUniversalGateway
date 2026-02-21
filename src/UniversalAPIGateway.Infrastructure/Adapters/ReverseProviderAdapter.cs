using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class ReverseProviderAdapter : IProviderAdapter
{
    public Provider Provider { get; } = new(
        new ProviderKey("reverse"),
        "Reverse Provider",
        ProviderCapability.TextGeneration);

    public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        var reversed = new string(payload.Reverse().ToArray());
        var response = new GatewayResponse(Provider.Key.Value, reversed);
        return Task.FromResult(response);
    }
}
