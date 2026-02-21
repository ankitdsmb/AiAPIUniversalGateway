using UniversalAPIGateway.Api.Contracts;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Api.Adapters;

public sealed class GatewayRequestAdapter : IGatewayRequestAdapter
{
    public bool TryAdapt(ExecuteAiRequest request, out GatewayRequest? gatewayRequest, out Dictionary<string, string[]> errors)
    {
        ArgumentNullException.ThrowIfNull(request);

        errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.ProviderKey))
        {
            errors[nameof(request.ProviderKey)] = ["ProviderKey is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Payload))
        {
            errors[nameof(request.Payload)] = ["Payload is required."];
        }

        if (errors.Count > 0)
        {
            gatewayRequest = null;
            return false;
        }

        gatewayRequest = new GatewayRequest(new ProviderKey(request.ProviderKey!), request.Payload!.Trim());
        return true;
    }
}
