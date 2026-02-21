namespace UniversalAPIGateway.Domain.Entities;

public sealed record GatewayRequest
{
    public ProviderKey ProviderKey { get; }
    public string Payload { get; }

    public GatewayRequest(ProviderKey providerKey, string payload)
    {
        if (string.IsNullOrWhiteSpace(providerKey.Value))
        {
            throw new ArgumentException("Provider key cannot be empty.", nameof(providerKey));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload cannot be empty.", nameof(payload));
        }

        ProviderKey = providerKey;
        Payload = payload.Trim();
    }
}
