namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderCooldownEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DateTimeOffset DisabledUntil { get; set; }
    public string Reason { get; set; } = string.Empty;

    public ProviderEntity Provider { get; set; } = null!;
}
