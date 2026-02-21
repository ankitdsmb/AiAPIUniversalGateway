using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Tests;

public sealed class ProviderSelectionStrategyScoringTests
{
    [Fact]
    public async Task SelectPrimaryAsync_UsesBestScore_WhenProviderKeyIsAuto()
    {
        var scoring = new TestProviderScoringService(new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["alpha"] = 15,
            ["beta"] = 41
        });

        var strategy = new DefaultProviderSelectionStrategy(scoring);
        var adapters = new IProviderAdapter[]
        {
            new SuccessAdapter(new ProviderKey("alpha")),
            new SuccessAdapter(new ProviderKey("beta"))
        };

        var selected = await strategy.SelectPrimaryAsync(adapters, new GatewayRequest(new ProviderKey("auto"), "payload"), CancellationToken.None);

        Assert.Equal("beta", selected.Provider.Key.Value);
    }

    [Fact]
    public async Task SelectFallbackAsync_SkipsDegradedProvider_WhenScoreLower()
    {
        var scoring = new TestProviderScoringService(new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["degraded"] = -25,
            ["healthy"] = 50
        });

        var strategy = new DefaultProviderSelectionStrategy(scoring);
        var degraded = new SuccessAdapter(new ProviderKey("degraded"));
        var healthy = new SuccessAdapter(new ProviderKey("healthy"));

        var selected = await strategy.SelectFallbackAsync(
            [degraded, healthy],
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
}
