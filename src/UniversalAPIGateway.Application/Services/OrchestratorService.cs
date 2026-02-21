using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class OrchestratorService : IOrchestratorService
{
    private readonly IReadOnlyList<IProviderAdapter> adapters;
    private readonly IProviderSelectionEngine providerSelectionEngine;
    private readonly IFallbackHandler fallbackHandler;
    private readonly IResponseNormalizer responseNormalizer;

    public OrchestratorService(
        IEnumerable<IProviderAdapter> adapters,
        IProviderSelectionEngine providerSelectionEngine,
        IFallbackHandler fallbackHandler,
        IResponseNormalizer responseNormalizer)
    {
        this.adapters = adapters?.ToArray() ?? throw new ArgumentNullException(nameof(adapters));
        if (this.adapters.Count == 0)
        {
            throw new InvalidOperationException("At least one provider adapter must be registered.");
        }

        this.providerSelectionEngine = providerSelectionEngine ?? throw new ArgumentNullException(nameof(providerSelectionEngine));
        this.fallbackHandler = fallbackHandler ?? throw new ArgumentNullException(nameof(fallbackHandler));
        this.responseNormalizer = responseNormalizer ?? throw new ArgumentNullException(nameof(responseNormalizer));
    }

    public async Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var primaryAdapter = await providerSelectionEngine.SelectPrimaryAsync(adapters, request, cancellationToken);
        var rawResponse = await fallbackHandler.ExecuteAsync(adapters, request, primaryAdapter, cancellationToken);

        return responseNormalizer.Normalize(rawResponse);
    }
}
