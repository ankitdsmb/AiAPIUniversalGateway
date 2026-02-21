using Microsoft.AspNetCore.Mvc;
using UniversalAPIGateway.Api.Adapters;
using UniversalAPIGateway.Api.Contracts;
using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Api.Controllers;

[ApiController]
[Route("v1/ai")]
public sealed class GatewayController(
    IGatewayService gatewayService,
    IGatewayRequestAdapter gatewayRequestAdapter) : ControllerBase
{
    [HttpPost("execute")]
    public async Task<ActionResult<ExecuteAiResponse>> ExecuteAsync([FromBody] ExecuteAiRequest request, CancellationToken cancellationToken)
    {
        if (!gatewayRequestAdapter.TryAdapt(request, out var gatewayRequest, out var errors))
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
            }

            return ValidationProblem(ModelState);
        }

        var gatewayResponse = await gatewayService.RouteAsync(gatewayRequest!, cancellationToken);
        return Ok(new ExecuteAiResponse(gatewayResponse.ProviderKey, gatewayResponse.Result));
    }
}
