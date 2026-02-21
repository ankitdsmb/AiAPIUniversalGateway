using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Strategies;

namespace UniversalAPIGateway.Application.Tests;

public sealed class GatewayServiceTests
{
    [Fact]
    public async Task RouteAsync_UsesConfiguredProvider()
    {
        var adapters = new[] { new EchoProviderAdapter(), new ReverseProviderAdapter() };
        var strategy = new ProviderSelectionStrategy(adapters);
        var sut = new GatewayService(strategy);

        var response = await sut.RouteAsync(new GatewayRequest("reverse", "abc"), CancellationToken.None);

        Assert.Equal("reverse", response.ProviderKey);
        Assert.Equal("cba", response.Result);
    }
}
