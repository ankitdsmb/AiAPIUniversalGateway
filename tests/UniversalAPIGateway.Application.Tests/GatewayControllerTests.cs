using Microsoft.AspNetCore.Mvc;
using UniversalAPIGateway.Api.Adapters;
using UniversalAPIGateway.Api.Contracts;
using UniversalAPIGateway.Api.Controllers;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Tests;

public sealed class GatewayControllerTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsOk_WhenRequestIsValid()
    {
        var gatewayService = new FakeGatewayService();
        var adapter = new GatewayRequestAdapter();
        var controller = new GatewayController(gatewayService, adapter);

        var result = await controller.ExecuteAsync(new ExecuteAiRequest("reverse", "hello"), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ExecuteAiResponse>(okResult.Value);

        Assert.Equal("reverse", response.ProviderKey);
        Assert.Equal("processed::hello", response.Result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsValidationProblem_WhenRequestIsInvalid()
    {
        var gatewayService = new FakeGatewayService();
        var adapter = new GatewayRequestAdapter();
        var controller = new GatewayController(gatewayService, adapter);

        var result = await controller.ExecuteAsync(new ExecuteAiRequest("  ", "  "), CancellationToken.None);

        var validationProblem = Assert.IsType<ObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(validationProblem.Value);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(nameof(ExecuteAiRequest.ProviderKey), controller.ModelState.Keys);
        Assert.Contains(nameof(ExecuteAiRequest.Payload), controller.ModelState.Keys);
    }

    private sealed class FakeGatewayService : IGatewayService
    {
        public Task<GatewayResponse> RouteAsync(GatewayRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new GatewayResponse(request.ProviderKey.Value, $"processed::{request.Payload}"));
    }
}
