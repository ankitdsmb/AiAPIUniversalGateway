using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class DefaultProviderSelectionStrategy(IProviderScoringService providerScoringService) : IProviderSelectionStrategy
{
    private const string AutoProviderKey = "auto";

    public async ValueTask<IProviderAdapter> SelectPrimaryAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ProviderKey.Value.Equals(AutoProviderKey, StringComparison.OrdinalIgnoreCase))
        {
            var bestAdapter = await SelectBestScoredAdapterAsync(
                adapters,
                ProviderCapability.TextGeneration,
                excludedAdapters: null,
                cancellationToken);

            return bestAdapter ?? throw new InvalidOperationException("No eligible provider is available for automatic selection.");
        }

        var adapter = adapters.FirstOrDefault(x =>
            x.Provider.Key.Value.Equals(request.ProviderKey.Value, StringComparison.OrdinalIgnoreCase)
            && x.Provider.IsEnabled);

        return adapter is null
            ? throw new InvalidOperationException($"Provider '{request.ProviderKey}' is not registered.")
            : adapter;
    }

    public ValueTask<IProviderAdapter?> SelectFallbackAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IReadOnlySet<IProviderAdapter> excludedAdapters,
        CancellationToken cancellationToken) =>
        SelectBestScoredAdapterAsync(adapters, ProviderCapability.TextGeneration, excludedAdapters, cancellationToken);

    private async ValueTask<IProviderAdapter?> SelectBestScoredAdapterAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        ProviderCapability requiredCapability,
        IReadOnlySet<IProviderAdapter>? excludedAdapters,
        CancellationToken cancellationToken)
    {
        IProviderAdapter? bestAdapter = null;
        var bestScore = double.NegativeInfinity;

        foreach (var adapter in adapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!adapter.Provider.IsEnabled || !adapter.Provider.Supports(requiredCapability))
            {
                continue;
            }

            if (excludedAdapters is not null && excludedAdapters.Contains(adapter))
            {
                continue;
            }

            var score = await providerScoringService.ScoreAsync(adapter.Provider, requiredCapability, cancellationToken);
            if (score > bestScore)
            {
                bestScore = score;
                bestAdapter = adapter;
            }
        }

        return bestAdapter;
    }
}
