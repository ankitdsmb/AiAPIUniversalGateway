namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderScoreEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    public ProviderEntity Provider { get; set; } = null!;
}
