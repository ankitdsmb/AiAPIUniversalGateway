using UniversalAPIGateway.Api.Contracts;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Api.Adapters;

public interface IGatewayRequestAdapter
{
    bool TryAdapt(ExecuteAiRequest request, out GatewayRequest? gatewayRequest, out Dictionary<string, string[]> errors);
}
