using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

public sealed class AdaptiveRoutingEngineTests
{
    [Fact]
    public async Task SelectAdapterAsync_PicksDifferentProviders_ByTaskTypePerformance()
    {
        var classifier = new KeywordTaskClassifier();
        var store = new InMemoryProviderPerformanceStore();
        var random = new StubRandomSource(0.95d, 0);
        var engine = new AdaptiveRoutingEngine(classifier, store, random);

        var codingProvider = new SuccessAdapter(new ProviderKey("coder"), ProviderCapability.TextGeneration);
        var chatProvider = new SuccessAdapter(new ProviderKey("chatter"), ProviderCapability.TextGeneration);
        var adapters = new IProviderAdapter[] { codingProvider, chatProvider };

        await SeedAsync(store, "coder", TaskType.Coding, successRate: 0.95, latencyMs: 180, quality: 0.9);
        await SeedAsync(store, "chatter", TaskType.Coding, successRate: 0.5, latencyMs: 600, quality: 0.5);
        await SeedAsync(store, "coder", TaskType.Chat, successRate: 0.4, latencyMs: 500, quality: 0.5);
        await SeedAsync(store, "chatter", TaskType.Chat, successRate: 0.92, latencyMs: 140, quality: 0.9);

        var codingSelection = await engine.SelectAdapterAsync(adapters, new GatewayRequest(new ProviderKey("auto"), "debug this code"), null, CancellationToken.None);
        var chatSelection = await engine.SelectAdapterAsync(adapters, new GatewayRequest(new ProviderKey("auto"), "hello how are you"), null, CancellationToken.None);

        Assert.Equal("coder", codingSelection?.Provider.Key.Value);
        Assert.Equal("chatter", chatSelection?.Provider.Key.Value);
    }

    [Fact]
    public async Task SelectAdapterAsync_UsesExplorationRate()
    {
        var classifier = new KeywordTaskClassifier();
        var store = new InMemoryProviderPerformanceStore();
        var random = new StubRandomSource(0.02d, 1);
        var engine = new AdaptiveRoutingEngine(classifier, store, random);

        var topProvider = new SuccessAdapter(new ProviderKey("top"), ProviderCapability.TextGeneration);
        var exploredProvider = new SuccessAdapter(new ProviderKey("explore"), ProviderCapability.TextGeneration);
        var adapters = new IProviderAdapter[] { topProvider, exploredProvider };

        await SeedAsync(store, "top", TaskType.Chat, successRate: 0.99, latencyMs: 100, quality: 0.95);
        await SeedAsync(store, "explore", TaskType.Chat, successRate: 0.3, latencyMs: 700, quality: 0.3);

        var selected = await engine.SelectAdapterAsync(adapters, new GatewayRequest(new ProviderKey("auto"), "chat with me"), null, CancellationToken.None);

        Assert.Equal("explore", selected?.Provider.Key.Value);
    }

    [Fact]
    public async Task SelectAdapterAsync_PrefersProviderWithSufficientHistory_WhenNewProviderHasFewSamples()
    {
        var classifier = new KeywordTaskClassifier();
        var store = new InMemoryProviderPerformanceStore();
        var random = new StubRandomSource(0.95d, 0);
        var engine = new AdaptiveRoutingEngine(classifier, store, random);

        var reliableProvider = new SuccessAdapter(new ProviderKey("reliable"), ProviderCapability.TextGeneration);
        var newProvider = new SuccessAdapter(new ProviderKey("new"), ProviderCapability.TextGeneration);
        var adapters = new IProviderAdapter[] { reliableProvider, newProvider };

        await SeedAsync(store, "reliable", TaskType.Chat, successRate: 0.9, latencyMs: 140, quality: 0.9);
        await store.UpdateOutcomeAsync("new", TaskType.Chat, succeeded: true, TimeSpan.FromMilliseconds(80), qualityScore: 1.0, CancellationToken.None);

        var selected = await engine.SelectAdapterAsync(adapters, new GatewayRequest(new ProviderKey("auto"), "hello how are you"), null, CancellationToken.None);

        Assert.Equal("reliable", selected?.Provider.Key.Value);
    }

    private static async Task SeedAsync(IProviderPerformanceStore store, string providerId, TaskType taskType, double successRate, int latencyMs, double quality)
    {
        var successSamples = (int)Math.Round(successRate * 10, MidpointRounding.AwayFromZero);
        for (var i = 0; i < 10; i++)
        {
            var success = i < successSamples;
            await store.UpdateOutcomeAsync(providerId, taskType, success, TimeSpan.FromMilliseconds(latencyMs), quality, CancellationToken.None);
        }
    }

    private sealed class SuccessAdapter(ProviderKey key, ProviderCapability capability) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, key.Value, capability);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            Task.FromResult(new GatewayResponse(Provider.Key.Value, payload));
    }

    private sealed class StubRandomSource(double nextDouble, int nextInt) : IRandomSource
    {
        public double NextDouble() => nextDouble;

        public int NextInt(int maxExclusive) => Math.Min(nextInt, Math.Max(0, maxExclusive - 1));
    }
}
