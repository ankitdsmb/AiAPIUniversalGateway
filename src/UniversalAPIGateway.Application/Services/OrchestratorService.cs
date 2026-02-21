using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class OrchestratorService(
    IEnumerable<IProviderAdapter> adapters,
    IProviderSelectionEngine providerSelectionEngine,
    IFallbackHandler fallbackHandler,
    IResponseNormalizer responseNormalizer) : IOrchestratorService
{
    public async Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var adapterCollection = adapters.ToArray();
        var primaryAdapter = await providerSelectionEngine.SelectPrimaryAsync(adapterCollection, request, cancellationToken);
        var rawResponse = await fallbackHandler.ExecuteAsync(adapterCollection, request, primaryAdapter, cancellationToken);

        return responseNormalizer.Normalize(rawResponse);
    }
}
