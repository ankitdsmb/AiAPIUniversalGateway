using System.Diagnostics;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class FallbackHandler : IFallbackHandler
{
    private readonly IProviderSelectionEngine selectionEngine;
    private readonly IProviderScoringService providerScoringService;
    private readonly IAdaptiveRoutingEngine adaptiveRoutingEngine;

    public FallbackHandler(
        IProviderSelectionEngine selectionEngine,
        IProviderScoringService providerScoringService,
        IAdaptiveRoutingEngine adaptiveRoutingEngine)
    {
        this.selectionEngine = selectionEngine ?? throw new ArgumentNullException(nameof(selectionEngine));
        this.providerScoringService = providerScoringService ?? throw new ArgumentNullException(nameof(providerScoringService));
        this.adaptiveRoutingEngine = adaptiveRoutingEngine ?? throw new ArgumentNullException(nameof(adaptiveRoutingEngine));
    }

    public async Task<GatewayResponse> ExecuteAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IProviderAdapter primaryAdapter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(adapters);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(primaryAdapter);

        if (adapters.Count == 0)
        {
            throw new InvalidOperationException("At least one provider adapter must be registered for fallback execution.");
        }

        if (!adapters.Contains(primaryAdapter))
        {
            throw new ArgumentException("The primary adapter must belong to the registered adapter collection.", nameof(primaryAdapter));
        }

        var excludedAdapters = new HashSet<IProviderAdapter>(ReferenceEqualityComparer.Instance);
        var currentAdapter = primaryAdapter;

        while (true)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await currentAdapter.ExecuteAsync(request.Payload, cancellationToken);
                stopwatch.Stop();

                await providerScoringService.RecordOutcomeAsync(
                    new RequestLog(Guid.NewGuid(), startedAt, currentAdapter.Provider.Key, true, stopwatch.Elapsed),
                    cancellationToken);

                await adaptiveRoutingEngine.RecordOutcomeAsync(
                    request,
                    currentAdapter.Provider.Key.Value,
                    succeeded: true,
                    latency: stopwatch.Elapsed,
                    responsePayload: response.Result,
                    cancellationToken);

                return response;
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                await providerScoringService.RecordOutcomeAsync(
                    new RequestLog(Guid.NewGuid(), startedAt, currentAdapter.Provider.Key, false, stopwatch.Elapsed),
                    cancellationToken);

                await adaptiveRoutingEngine.RecordOutcomeAsync(
                    request,
                    currentAdapter.Provider.Key.Value,
                    succeeded: false,
                    latency: stopwatch.Elapsed,
                    responsePayload: null,
                    cancellationToken);

                excludedAdapters.Add(currentAdapter);

                var fallbackAdapter = await selectionEngine.SelectFallbackAsync(adapters, request, excludedAdapters, cancellationToken);
                if (fallbackAdapter is null)
                {
                    throw;
                }

                currentAdapter = fallbackAdapter;
            }
        }
    }
}
