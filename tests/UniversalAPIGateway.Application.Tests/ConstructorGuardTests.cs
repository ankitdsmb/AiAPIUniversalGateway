using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

public sealed class ConstructorGuardTests
{
    [Fact]
    public void OrchestratorService_NullDependencies_ThrowArgumentNullException()
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine, new TestProviderScoringService(), new TestAdaptiveRoutingEngine());
        var responseNormalizer = new ResponseNormalizer();
        var adapter = new MockProviderAdapter();

        Assert.Throws<ArgumentNullException>(() => new OrchestratorService(null!, selectionEngine, fallbackHandler, responseNormalizer));
        Assert.Throws<ArgumentNullException>(() => new OrchestratorService([adapter], null!, fallbackHandler, responseNormalizer));
        Assert.Throws<ArgumentNullException>(() => new OrchestratorService([adapter], selectionEngine, null!, responseNormalizer));
        Assert.Throws<ArgumentNullException>(() => new OrchestratorService([adapter], selectionEngine, fallbackHandler, null!));
    }

    [Fact]
    public void ProviderSelectionEngine_NullStrategy_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ProviderSelectionEngine(null!));
    }

    [Fact]
    public void FallbackHandler_NullDependencies_ThrowArgumentNullException()
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var scoringService = new TestProviderScoringService();
        var adaptiveRoutingEngine = new TestAdaptiveRoutingEngine();

        Assert.Throws<ArgumentNullException>(() => new FallbackHandler(null!, scoringService, adaptiveRoutingEngine));
        Assert.Throws<ArgumentNullException>(() => new FallbackHandler(selectionEngine, null!, adaptiveRoutingEngine));
        Assert.Throws<ArgumentNullException>(() => new FallbackHandler(selectionEngine, scoringService, null!));
    }

    private sealed class MockProviderAdapter : IProviderAdapter
    {
        public Provider Provider { get; } = new(new ProviderKey("mock"), "Mock", ProviderCapability.TextGeneration);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            Task.FromResult(new GatewayResponse(Provider.Key.Value, payload));
    }
}
