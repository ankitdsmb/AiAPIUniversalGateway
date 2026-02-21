using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class EchoProviderAdapter : IProviderAdapter
{
    public Provider Provider { get; } = new(
        new ProviderKey("echo"),
        "Echo Provider",
        ProviderCapability.TextGeneration);

    public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        var response = new GatewayResponse(Provider.Key.Value, payload);
        return Task.FromResult(response);
    }
}
