namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderTelemetryEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int LatencyMs { get; set; }
    public long Tokens { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public ProviderEntity Provider { get; set; } = null!;
}
