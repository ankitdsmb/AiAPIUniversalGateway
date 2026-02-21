namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderKeyEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public long QuotaLimit { get; set; }
    public long QuotaUsed { get; set; }
    public string ResetPeriod { get; set; } = "daily";

    public ProviderEntity Provider { get; set; } = null!;
}
