using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Services;

public sealed class ResponseNormalizer : IResponseNormalizer
{
    public GatewayResponse Normalize(GatewayResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (string.IsNullOrWhiteSpace(response.Result))
        {
            throw new InvalidOperationException("Provider returned an empty response payload.");
        }

        var normalizedResult = response.Result.Trim();
        return new GatewayResponse(response.ProviderKey, normalizedResult);
    }
}
