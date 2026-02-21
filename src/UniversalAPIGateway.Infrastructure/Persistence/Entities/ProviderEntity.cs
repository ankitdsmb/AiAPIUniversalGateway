namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Capabilities { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ProviderKeyEntity> ProviderKeys { get; set; } = new List<ProviderKeyEntity>();
    public ICollection<ProviderHealthEntity> HealthChecks { get; set; } = new List<ProviderHealthEntity>();
    public ICollection<ProviderScoreEntity> Scores { get; set; } = new List<ProviderScoreEntity>();
    public ICollection<GatewayResponseEntity> GatewayResponses { get; set; } = new List<GatewayResponseEntity>();
    public ICollection<ProviderTelemetryEntity> Telemetry { get; set; } = new List<ProviderTelemetryEntity>();
    public ICollection<ProviderPerformanceEntity> Performances { get; set; } = new List<ProviderPerformanceEntity>();
    public ICollection<RoutingDecisionEntity> RoutingDecisions { get; set; } = new List<RoutingDecisionEntity>();
    public ICollection<ProviderFailureEntity> Failures { get; set; } = new List<ProviderFailureEntity>();
    public ICollection<ProviderCooldownEntity> Cooldowns { get; set; } = new List<ProviderCooldownEntity>();
}
