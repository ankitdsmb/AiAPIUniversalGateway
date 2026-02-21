namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ApiClientEntity
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int RateLimit { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ClientUsageStatEntity> UsageStats { get; set; } = new List<ClientUsageStatEntity>();
}
