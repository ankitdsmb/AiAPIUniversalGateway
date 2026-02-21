using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

public sealed class ProviderSelectionStrategyScoringTests
{
    [Fact]
    public async Task SelectPrimaryAsync_UsesAdaptiveEngine_WhenProviderKeyIsAuto()
    {
        var expected = new SuccessAdapter(new ProviderKey("beta"));
        var strategy = new DefaultProviderSelectionStrategy(new StubAdaptiveRoutingEngine(expected));
        var adapters = new IProviderAdapter[]
        {
            new SuccessAdapter(new ProviderKey("alpha")),
            expected
        };

        var selected = await strategy.SelectPrimaryAsync(adapters, new GatewayRequest(new ProviderKey("auto"), "payload"), CancellationToken.None);

        Assert.Equal("beta", selected.Provider.Key.Value);
    }

    [Fact]
    public async Task SelectFallbackAsync_UsesAdaptiveEngine()
    {
        var expected = new SuccessAdapter(new ProviderKey("healthy"));
        var strategy = new DefaultProviderSelectionStrategy(new StubAdaptiveRoutingEngine(expected));

        var selected = await strategy.SelectFallbackAsync(
            [new SuccessAdapter(new ProviderKey("degraded")), expected],
            new GatewayRequest(new ProviderKey("reverse"), "payload"),
            new HashSet<IProviderAdapter>(ReferenceEqualityComparer.Instance),
            CancellationToken.None);

        Assert.NotNull(selected);
        Assert.Equal("healthy", selected.Provider.Key.Value);
    }

    private sealed class SuccessAdapter(ProviderKey key) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, key.Value, ProviderCapability.TextGeneration);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            Task.FromResult(new GatewayResponse(Provider.Key.Value, payload));
    }

    private sealed class StubAdaptiveRoutingEngine(IProviderAdapter? selectedAdapter) : IAdaptiveRoutingEngine
    {
        public ValueTask<IProviderAdapter?> SelectAdapterAsync(IReadOnlyCollection<IProviderAdapter> adapters, GatewayRequest request, IReadOnlySet<IProviderAdapter>? excludedAdapters, CancellationToken cancellationToken) =>
            ValueTask.FromResult(selectedAdapter);

        public ValueTask RecordOutcomeAsync(GatewayRequest request, string providerId, bool succeeded, TimeSpan latency, string? responsePayload, int tokenUsage, CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;
    }
}
