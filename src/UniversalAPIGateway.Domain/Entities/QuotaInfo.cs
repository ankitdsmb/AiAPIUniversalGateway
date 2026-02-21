namespace UniversalAPIGateway.Domain.Entities;

public sealed record QuotaInfo(
    string Subject,
    long DailyLimit,
    long DailyUsed,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd)
{
    public long Remaining => Math.Max(0, DailyLimit - DailyUsed);
    public bool HasCapacity(long units) => units <= Remaining;
}
