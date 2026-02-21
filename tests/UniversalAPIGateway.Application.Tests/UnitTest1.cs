using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

public sealed class GatewayServiceTests
{
    [Fact]
    public async Task RouteAsync_UsesConfiguredProvider()
    {
        var adapter = new TestAdapter(new ProviderKey("reverse"));
        var strategy = new TestSelector(adapter);
        var sut = new GatewayService(new[] { adapter }, strategy);

        var response = await sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "abc"), CancellationToken.None);

        Assert.Equal("reverse", response.ProviderKey);
        Assert.Equal("cba", response.Result);
    }

    private sealed class TestSelector(IProviderAdapter adapter) : IProviderSelector
    {
        public ValueTask<IProviderAdapter> SelectAsync(
            IEnumerable<IProviderAdapter> adapters,
            ProviderKey preferredProvider,
            ProviderCapability requiredCapability,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(adapter);
    }

    private sealed class TestAdapter(ProviderKey key) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, "test", ProviderCapability.TextGeneration);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            Task.FromResult(new GatewayResponse(Provider.Key.Value, new string(payload.Reverse().ToArray())));
    }
}
