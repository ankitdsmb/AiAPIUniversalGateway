using UniversalAPIGateway.Api.Adapters;
using UniversalAPIGateway.Api.Contracts;

namespace UniversalAPIGateway.Application.Tests;

public sealed class GatewayRequestAdapterTests
{
    [Fact]
    public void TryAdapt_TrimsValues_WhenRequestIsValid()
    {
        var sut = new GatewayRequestAdapter();

        var isValid = sut.TryAdapt(new ExecuteAiRequest(" reverse ", " payload "), out var gatewayRequest, out var errors);

        Assert.True(isValid);
        Assert.Empty(errors);
        Assert.NotNull(gatewayRequest);
        Assert.Equal("reverse", gatewayRequest.ProviderKey.Value);
        Assert.Equal("payload", gatewayRequest.Payload);
    }
}
