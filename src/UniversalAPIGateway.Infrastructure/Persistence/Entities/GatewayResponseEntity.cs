namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class GatewayResponseEntity
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public bool Success { get; set; }
    public int LatencyMs { get; set; }
    public long TokenUsage { get; set; }
    public string? ErrorType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public GatewayRequestEntity Request { get; set; } = null!;
    public ProviderEntity Provider { get; set; } = null!;
}
