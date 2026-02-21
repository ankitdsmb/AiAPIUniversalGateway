using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IOrchestratorService
{
    Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken);
}
