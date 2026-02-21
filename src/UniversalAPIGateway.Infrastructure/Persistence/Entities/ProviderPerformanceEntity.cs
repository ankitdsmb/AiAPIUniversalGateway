namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderPerformanceEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public decimal SuccessRate { get; set; }
    public int AvgLatency { get; set; }
    public decimal QualityScore { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ProviderEntity Provider { get; set; } = null!;
}
