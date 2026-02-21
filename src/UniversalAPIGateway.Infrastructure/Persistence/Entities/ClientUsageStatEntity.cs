namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ClientUsageStatEntity
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public long RequestsCount { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }

    public ApiClientEntity Client { get; set; } = null!;
}
