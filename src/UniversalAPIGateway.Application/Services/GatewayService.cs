using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Services;

public sealed class GatewayService(IProviderSelectionStrategy providerSelectionStrategy) : IGatewayService
{
    public Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var adapter = providerSelectionStrategy.Resolve(request.ProviderKey);
        return adapter.ExecuteAsync(request.Payload, cancellationToken);
    }
}
