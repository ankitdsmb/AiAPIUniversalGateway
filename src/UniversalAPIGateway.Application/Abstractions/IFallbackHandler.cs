using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Application.Abstractions;

public interface IFallbackHandler
{
    Task<GatewayResponse> ExecuteAsync(
        IReadOnlyCollection<IProviderAdapter> adapters,
        GatewayRequest request,
        IProviderAdapter primaryAdapter,
        CancellationToken cancellationToken);
}
