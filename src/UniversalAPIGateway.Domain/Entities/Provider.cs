namespace UniversalAPIGateway.Domain.Entities;

public sealed record Provider(
    ProviderKey Key,
    string DisplayName,
    ProviderCapability Capabilities,
    bool IsEnabled = true)
{
    public bool Supports(ProviderCapability capability) =>
        capability == ProviderCapability.None || Capabilities.HasFlag(capability);
}
