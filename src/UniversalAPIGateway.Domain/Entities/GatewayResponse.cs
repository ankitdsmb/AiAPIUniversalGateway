namespace UniversalAPIGateway.Domain.Entities;

public sealed record GatewayResponse
{
    public string ProviderKey { get; }
    public string Result { get; }

    public GatewayResponse(string providerKey, string result)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            throw new ArgumentException("Provider key cannot be empty.", nameof(providerKey));
        }

        ProviderKey = providerKey.Trim();
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }
}
