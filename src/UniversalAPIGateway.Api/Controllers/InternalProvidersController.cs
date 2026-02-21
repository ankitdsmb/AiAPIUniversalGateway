using Microsoft.AspNetCore.Mvc;
using UniversalAPIGateway.Api.Contracts;
using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Api.Controllers;

[ApiController]
[Route("internal/providers")]
public sealed class InternalProvidersController(IProviderRegistryService providerRegistryService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ProviderRegistryResponse>> RegisterAsync(
        [FromBody] RegisterProviderRequest request,
        CancellationToken cancellationToken)
    {
        var entry = await providerRegistryService.RegisterAsync(
            new ProviderRegistration(request.ProviderKey, request.DisplayName, request.Endpoint, request.Capabilities),
            cancellationToken);

        return Ok(ToResponse(entry));
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<ProviderRegistryResponse>> HeartbeatAsync(
        [FromBody] ProviderHeartbeatRequest request,
        CancellationToken cancellationToken)
    {
        var entry = await providerRegistryService.HeartbeatAsync(request.ProviderKey, cancellationToken);

        return entry is null
            ? NotFound($"Provider '{request.ProviderKey}' is not registered.")
            : Ok(ToResponse(entry));
    }

    private static ProviderRegistryResponse ToResponse(ProviderRegistryEntry entry) =>
        new(
            entry.ProviderKey,
            entry.DisplayName,
            entry.Endpoint,
            entry.Capabilities,
            entry.IsEnabled,
            entry.LastHeartbeatUtc,
            entry.UpdatedAtUtc);
}
