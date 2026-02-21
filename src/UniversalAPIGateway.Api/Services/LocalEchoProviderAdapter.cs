using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Api.Services;

public sealed class LocalEchoProviderAdapter : IProviderAdapter
{
    public Provider Provider { get; } = new(new ProviderKey("local-echo"), "Local Echo", ProviderCapability.TextGeneration);

    public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new GatewayResponse(Provider.Key.Value, payload));
    }
}
