namespace UniversalAPIGateway.Infrastructure.Persistence.Entities;

public sealed class ProviderFailureEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }

    public ProviderEntity Provider { get; set; } = null!;
}
