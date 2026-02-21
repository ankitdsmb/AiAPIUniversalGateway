using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class MockProviderAdapter : IProviderAdapter
{
    public Provider Provider { get; } = new(
        new ProviderKey("mock"),
        "Mock Provider",
        ProviderCapability.TextGeneration);

    public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new GatewayResponse(Provider.Key.Value, $"mock::{payload}"));
    }
}
