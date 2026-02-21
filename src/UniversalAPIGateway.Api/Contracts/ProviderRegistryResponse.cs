namespace UniversalAPIGateway.Api.Contracts;

public sealed record ProviderRegistryResponse(
    string ProviderKey,
    string DisplayName,
    string Endpoint,
    string Capabilities,
    bool IsEnabled,
    DateTimeOffset LastHeartbeatUtc,
    DateTimeOffset UpdatedAtUtc);
