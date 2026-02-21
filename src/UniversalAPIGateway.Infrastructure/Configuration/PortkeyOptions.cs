namespace UniversalAPIGateway.Infrastructure.Configuration;

public sealed class PortkeyOptions
{
    public const string SectionName = "Providers:Portkey";

    public string BaseUrl { get; init; } = "https://api.portkey.ai";

    public string ApiKey { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 5;
}
