using Microsoft.AspNetCore.Mvc;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Api.Controllers;

[ApiController]
[Route("api/gateway")]
public sealed class GatewayController(IGatewayService gatewayService) : ControllerBase
{
    [HttpPost("route")]
    public async Task<ActionResult<GatewayResponse>> RouteAsync([FromBody] GatewayRequest request, CancellationToken cancellationToken)
    {
        var response = await gatewayService.RouteAsync(request, cancellationToken);
        return Ok(response);
    }
}
