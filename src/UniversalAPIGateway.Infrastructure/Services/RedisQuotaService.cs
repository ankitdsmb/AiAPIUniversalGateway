using StackExchange.Redis;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Services;

public sealed class RedisQuotaService(IConnectionMultiplexer connectionMultiplexer) : IQuotaService
{
    private const long DailyLimit = 10_000;

    private readonly IDatabase database = connectionMultiplexer.GetDatabase();

    public async ValueTask<QuotaInfo> GetQuotaAsync(string subject, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildUsageKey(subject);
        var used = (long)await database.StringGetAsync(key);
        var (windowStart, windowEnd) = GetWindow();

        return new QuotaInfo(subject, DailyLimit, used, windowStart, windowEnd);
    }

    public async ValueTask<bool> TryReserveAsync(string subject, long units, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildUsageKey(subject);
        var reserved = await database.StringIncrementAsync(key, units);

        if (reserved == units)
        {
            await database.KeyExpireAsync(key, TimeSpan.FromDays(1));
        }

        if (reserved > DailyLimit)
        {
            await database.StringDecrementAsync(key, units);
            return false;
        }

        return true;
    }

    public async ValueTask RecordUsageAsync(string subject, long units, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildUsageKey(subject);
        await database.StringIncrementAsync(key, units);
        await database.KeyExpireAsync(key, TimeSpan.FromDays(1));
    }

    private static string BuildUsageKey(string subject)
    {
        var now = DateTimeOffset.UtcNow;
        return $"quota:{subject}:{now:yyyyMMdd}";
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetWindow()
    {
        var now = DateTimeOffset.UtcNow;
        var start = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(1);
        return (start, end);
    }
}
