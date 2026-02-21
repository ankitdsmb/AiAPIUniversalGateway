namespace UniversalAPIGateway.Infrastructure.Configuration;

public sealed class ProviderIntelligenceOptions
{
    public const string SectionName = "ProviderIntelligence";

    public double SuccessRateWeight { get; init; } = 50;
    public double QuotaRemainingWeight { get; init; } = 30;
    public double LatencyWeight { get; init; } = 10;
    public double RecentFailuresWeight { get; init; } = 20;
    public double MaxLatencyMilliseconds { get; init; } = 5_000;
    public double MaxRecentFailures { get; init; } = 10;
}
