using System.Diagnostics;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class FallbackHandler(IProviderSelectionEngine selectionEngine, IProviderScoringService providerScoringService) : IFallbackHandler
{
    public async Task<GatewayResponse> ExecuteAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IProviderAdapter primaryAdapter,
        CancellationToken cancellationToken)
    {
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

                return response;
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                await providerScoringService.RecordOutcomeAsync(
                    new RequestLog(Guid.NewGuid(), startedAt, currentAdapter.Provider.Key, false, stopwatch.Elapsed),
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
