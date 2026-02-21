namespace UniversalAPIGateway.Api.Contracts;

public sealed record RegisterProviderRequest(
    string ProviderKey,
    string DisplayName,
    string Endpoint,
    IReadOnlyCollection<string> Capabilities);
