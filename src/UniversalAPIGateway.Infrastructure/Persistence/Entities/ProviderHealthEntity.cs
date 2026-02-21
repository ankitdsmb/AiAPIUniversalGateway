namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderHealthEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int AvgLatency { get; set; }
    public int FailureCount { get; set; }
    public DateTimeOffset LastChecked { get; set; }

    public ProviderEntity Provider { get; set; } = null!;
}
