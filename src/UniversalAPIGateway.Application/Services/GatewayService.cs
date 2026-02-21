using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Services;

public sealed class GatewayService(
    IEnumerable<IProviderAdapter> adapters,
    IProviderSelector providerSelector) : IGatewayService
{
    public async Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var selectedAdapter = await providerSelector.SelectAsync(
            adapters,
            request.ProviderKey,
            ProviderCapability.None,
            cancellationToken);

        return await selectedAdapter.ExecuteAsync(request.Payload, cancellationToken);
    }
}
