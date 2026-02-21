namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class GatewayRequestEntity
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public string OutputType { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<GatewayResponseEntity> Responses { get; set; } = new List<GatewayResponseEntity>();
    public ICollection<RoutingDecisionEntity> RoutingDecisions { get; set; } = new List<RoutingDecisionEntity>();
}
