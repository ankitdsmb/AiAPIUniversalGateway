using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class FallbackHandler(IProviderSelectionEngine selectionEngine) : IFallbackHandler
{
    public async Task<GatewayResponse> ExecuteAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IProviderAdapter primaryAdapter,
        CancellationToken cancellationToken)
    {
        try
        {
            return await primaryAdapter.ExecuteAsync(request.Payload, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            var fallbackAdapter = await selectionEngine.SelectFallbackAsync(adapters, request, primaryAdapter, cancellationToken);

            if (fallbackAdapter is null)
            {
                throw;
            }

            return await fallbackAdapter.ExecuteAsync(request.Payload, cancellationToken);
        }
    }
}
