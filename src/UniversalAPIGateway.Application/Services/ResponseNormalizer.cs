using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Services;

public sealed class ResponseNormalizer : IResponseNormalizer
{
    public GatewayResponse Normalize(GatewayResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var normalizedResult = response.Result.Trim();
        return response with { Result = normalizedResult };
    }
}
