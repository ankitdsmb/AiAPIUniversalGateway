using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;

namespace UniversalAPIGateway.Application.Tests;

public sealed class QaValidationScenariosTests
{
    [Fact]
    public void QuotaExceeded_ReturnsNoCapacity()
    {
        var quota = new QuotaInfo("tenant-qa", 100, 100, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));

        var hasCapacity = quota.HasCapacity(1);

        Assert.False(hasCapacity);
    }

    [Fact]
    public async Task ProviderTimeout_ThrowsTimeoutRejectedException()
    {
        var options = Options.Create(new PortkeyOptions
        {
            BaseUrl = "https://unit.test/",
            TimeoutSeconds = 1,
            ApiKey = string.Empty
        });

        var slowHandler = new DelayedResponseHandler(TimeSpan.FromSeconds(2));
        var httpClient = new HttpClient(slowHandler)
        {
            BaseAddress = new Uri(options.Value.BaseUrl)
        };

        var sut = new PortkeyAdapter(httpClient, options, new ProviderResiliencePolicies());

        await Assert.ThrowsAsync<TimeoutRejectedException>(() => sut.ExecuteAsync("payload", CancellationToken.None));
    }

    [Fact]
    public async Task FallbackSuccess_UsesFallbackProvider()
    {
        var primary = new ThrowingAdapter(new ProviderKey("reverse"));
        var fallback = new SuccessAdapter(new ProviderKey("echo"), payload => payload);
        var sut = CreateOrchestrator(primary, fallback);

        var response = await sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "resilient"), CancellationToken.None);

        Assert.Equal("echo", response.ProviderKey);
        Assert.Equal("resilient", response.Result);
    }


    [Fact]
    public async Task FallbackFailure_UsesNextAvailableProvider()
    {
        var primary = new ThrowingAdapter(new ProviderKey("reverse"));
        var firstFallback = new ThrowingAdapter(new ProviderKey("echo"));
        var secondFallback = new SuccessAdapter(new ProviderKey("mock"), payload => $"ok::{payload}");
        var sut = CreateOrchestrator(primary, firstFallback, secondFallback);

        var response = await sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "resilient"), CancellationToken.None);

        Assert.Equal("mock", response.ProviderKey);
        Assert.Equal("ok::resilient", response.Result);
    }

    [Fact]
    public void OrchestratorWithoutAdapters_ThrowsInvalidOperationException()
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine, new TestProviderScoringService(), new TestAdaptiveRoutingEngine());
        var responseNormalizer = new ResponseNormalizer();

        var error = Assert.Throws<InvalidOperationException>(() =>
            new OrchestratorService(Array.Empty<IProviderAdapter>(), selectionEngine, fallbackHandler, responseNormalizer));

        Assert.Contains("At least one provider adapter", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidProviderResponse_ThrowsArgumentNullException()
    {
        var primary = new SuccessAdapter(new ProviderKey("broken"), _ => null!);
        var sut = CreateOrchestrator(primary);

        var error = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.RouteAsync(new GatewayRequest(new ProviderKey("broken"), "payload"), CancellationToken.None));

        Assert.Contains("result", error.ParamName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HighLatencyRouting_CompletesThroughPrimaryProvider()
    {
        var primary = new DelayedSuccessAdapter(new ProviderKey("reverse"), TimeSpan.FromMilliseconds(175), payload => payload.ToUpperInvariant());
        var fallback = new SuccessAdapter(new ProviderKey("echo"), payload => payload);
        var sut = CreateOrchestrator(primary, fallback);
        var stopwatch = Stopwatch.StartNew();

        var response = await sut.RouteAsync(new GatewayRequest(new ProviderKey("reverse"), "latency"), CancellationToken.None);

        stopwatch.Stop();

        Assert.Equal("reverse", response.ProviderKey);
        Assert.Equal("LATENCY", response.Result);
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(150));
    }

    [Fact]
    public async Task FallbackHandler_WithEmptyAdapters_ThrowsInvalidOperationException()
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine, new TestProviderScoringService(), new TestAdaptiveRoutingEngine());
        var request = new GatewayRequest(new ProviderKey("reverse"), "payload");
        var primary = new SuccessAdapter(new ProviderKey("reverse"), payload => payload);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fallbackHandler.ExecuteAsync(Array.Empty<IProviderAdapter>(), request, primary, CancellationToken.None));

        Assert.Contains("At least one provider adapter", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FallbackHandler_WithNullRequest_ThrowsArgumentNullException()
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine, new TestProviderScoringService(), new TestAdaptiveRoutingEngine());
        var primary = new SuccessAdapter(new ProviderKey("reverse"), payload => payload);
        var adapters = new IProviderAdapter[] { primary };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            fallbackHandler.ExecuteAsync(adapters, null!, primary, CancellationToken.None));
    }

    [Fact]
    public async Task FallbackHandler_WithPrimaryOutsideAdapterCollection_ThrowsArgumentException()
    {
        var strategy = new DefaultProviderSelectionStrategy(new TestAdaptiveRoutingEngine());
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine, new TestProviderScoringService(), new TestAdaptiveRoutingEngine());
        var registeredAdapter = new SuccessAdapter(new ProviderKey("reverse"), payload => payload);
        var unregisteredPrimary = new SuccessAdapter(new ProviderKey("echo"), payload => payload);
        var request = new GatewayRequest(new ProviderKey("reverse"), "payload");

        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            fallbackHandler.ExecuteAsync(new IProviderAdapter[] { registeredAdapter }, request, unregisteredPrimary, CancellationToken.None));

        Assert.Equal("primaryAdapter", error.ParamName);
    }

    private static OrchestratorService CreateOrchestrator(params IProviderAdapter[] adapters)
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

    private sealed class DelayedSuccessAdapter(ProviderKey key, TimeSpan delay, Func<string, string> responseFactory) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, key.Value, ProviderCapability.TextGeneration);

        public async Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return new GatewayResponse(Provider.Key.Value, responseFactory(payload));
        }
    }

    private sealed class ThrowingAdapter(ProviderKey key) : IProviderAdapter
    {
        public Provider Provider { get; } = new(key, key.Value, ProviderCapability.TextGeneration);

        public Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Primary provider failed.");
    }

    private sealed class DelayedResponseHandler(TimeSpan delay) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"ok\"}", Encoding.UTF8, "application/json")
            };
        }
    }
}
