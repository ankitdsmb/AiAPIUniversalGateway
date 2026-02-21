namespace UniversalAPIGateway.Infrastructure.Configuration;

public sealed class ProviderEndpointOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 15;

    public int CooldownSeconds { get; init; } = 120;
}
