namespace UniversalAPIGateway.Domain.Entities;

public sealed record ProviderPerformance(
    string ProviderId,
    TaskType TaskType,
    double SuccessRate,
    TimeSpan Latency,
    double QualityScore,
    long SampleSize = 0)
{
    public static ProviderPerformance CreateDefault(string providerId, TaskType taskType) =>
        new(providerId, taskType, SuccessRate: 0.5d, Latency: TimeSpan.FromMilliseconds(500), QualityScore: 0.5d, SampleSize: 0);
}
