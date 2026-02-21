using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IGatewayService
{
    Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken);
}
