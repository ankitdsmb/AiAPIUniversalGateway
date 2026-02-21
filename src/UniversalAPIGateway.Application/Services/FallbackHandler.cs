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
        var excludedAdapters = new HashSet<IProviderAdapter>(ReferenceEqualityComparer.Instance);
        var currentAdapter = primaryAdapter;

        while (true)
        {
            try
            {
                return await currentAdapter.ExecuteAsync(request.Payload, cancellationToken);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
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
