namespace UniversalAPIGateway.Application.Abstractions;

public interface IProviderRegistryService
{
    Task<ProviderRegistryEntry> RegisterAsync(ProviderRegistration registration, CancellationToken cancellationToken);

    Task<ProviderRegistryEntry?> HeartbeatAsync(string providerKey, CancellationToken cancellationToken);
}

public sealed record ProviderRegistration(
    string ProviderKey,
    string DisplayName,
    string Endpoint,
    IReadOnlyCollection<string> Capabilities);

public sealed record ProviderRegistryEntry(
    string ProviderKey,
    string DisplayName,
    string Endpoint,
    string Capabilities,
    bool IsEnabled,
    DateTimeOffset LastHeartbeatUtc,
    DateTimeOffset UpdatedAtUtc);

