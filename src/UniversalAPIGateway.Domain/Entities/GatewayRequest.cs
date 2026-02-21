namespace UniversalAPIGateway.Domain.Entities;

public sealed record GatewayRequest(ProviderKey ProviderKey, string Payload);
