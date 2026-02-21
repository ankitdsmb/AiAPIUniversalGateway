using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

public sealed class OrchestratorServiceTests
{
    [Fact]
    public async Task RouteAsync_UsesPrimaryAdapter_WhenPrimarySucceeds()
    {
        var primary = new SuccessAdapter(new ProviderKey("reverse"), payload => new string(payload.Reverse().ToArray()));
        var fallback = new SuccessAdapter(new ProviderKey("echo"), payload => payload);
        var sut = CreateSut(primary, fallback);

        var response = await sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "abc"), CancellationToken.None);

        Assert.Equal("reverse", response.ProviderKey);
        Assert.Equal("cba", response.Result);
    }

    [Fact]
    public async Task RouteAsync_FallsBack_WhenPrimaryThrows()
    {
        var primary = new ThrowingAdapter(new ProviderKey("reverse"));
        var fallback = new SuccessAdapter(new ProviderKey("echo"), payload => $"  {payload}  ");
        var sut = CreateSut(primary, fallback);

        var response = await sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "abc"), CancellationToken.None);

        Assert.Equal("echo", response.ProviderKey);
        Assert.Equal("abc", response.Result);
    }

    [Fact]
    public async Task RouteAsync_Throws_WhenNoFallbackAvailable()
    {
        var primary = new ThrowingAdapter(new ProviderKey("reverse"));
        var sut = CreateSut(primary);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "abc"), CancellationToken.None));
    }


    [Fact]
    public async Task RouteAsync_PropagatesOperationCanceledException_WhenCancellationRequested()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var primary = new SuccessAdapter(new ProviderKey("reverse"), payload => payload);
        var fallback = new SuccessAdapter(new ProviderKey("echo"), payload => payload);
        var sut = CreateSut(primary, fallback);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "abc"), cts.Token));
    }

    private static OrchestratorService CreateSut(params IProviderAdapter[] adapters)
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine, new TestProviderScoringService(), new TestAdaptiveRoutingEngine());
        var responseNormalizer = new ResponseNormalizer();

        return new OrchestratorService(adapters, selectionEngine, fallbackHandler, responseNormalizer);
    }

    private sealed class SuccessAdapter(ProviderKey key, Func<string, string> responseFactory) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, key.Value, ProviderCapability.TextGeneration);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            Task.FromResult(new GatewayResponse(Provider.Key.Value, responseFactory(payload)));
    }

    private sealed class ThrowingAdapter(ProviderKey key) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, key.Value, ProviderCapability.TextGeneration);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Primary provider failed.");
    }
}
