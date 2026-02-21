using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Services;

public sealed class GatewayService(IOrchestratorService orchestratorService) : IGatewayService
{
    public Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken) =>
        orchestratorService.RouteAsync(request, cancellationToken);
}
