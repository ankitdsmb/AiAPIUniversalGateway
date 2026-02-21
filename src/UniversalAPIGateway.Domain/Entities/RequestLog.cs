namespace UniversalAPIGateway.Domain.Entities;

public sealed record RequestLog(
    Guid RequestId,
    DateTimeOffset OccurredAt,
    ProviderKey ProviderKey,
    bool Succeeded,
    TimeSpan Duration,
    string? ErrorCode = null,
    long UnitsConsumed = 0);
