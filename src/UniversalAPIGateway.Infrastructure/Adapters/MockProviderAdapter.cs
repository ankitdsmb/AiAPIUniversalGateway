using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class MockProviderAdapter : IProviderAdapter
{
    public Provider Provider { get; } = new(
        new ProviderKey("mock"),
        "Mock Provider",
        ProviderCapability.TextGeneration);

    public async Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();

        return new GatewayResponse(Provider.Key.Value, $"mock::{payload}");
    }
}
