namespace UniversalAPIGateway.Domain.Entities;

public sealed record GatewayRequest(string ProviderKey, string Payload);
