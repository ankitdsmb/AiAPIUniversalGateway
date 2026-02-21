using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IResponseNormalizer
{
    GatewayResponse Normalize(GatewayResponse response);
}
