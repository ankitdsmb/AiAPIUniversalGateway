namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class RoutingDecisionEntity
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public Guid ChosenProviderId { get; set; }
    public string CandidateProviders { get; set; } = "[]";
    public decimal DecisionScore { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public GatewayRequestEntity Request { get; set; } = null!;
    public ProviderEntity ChosenProvider { get; set; } = null!;
}
