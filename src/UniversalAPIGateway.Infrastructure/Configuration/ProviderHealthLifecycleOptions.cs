namespace UniversalAPIGateway.Infrastructure.Configuration;

public sealed class ProviderHealthLifecycleOptions
{
    public const string SectionName = "ProviderHealthLifecycle";

    public int FailureThreshold { get; init; } = 3;

    public int RecoverySuccessThreshold { get; init; } = 2;

    public int CooldownSeconds { get; init; } = 120;

    public int DegradedFailureThreshold { get; init; } = 1;

    public int HealthCheckIntervalSeconds { get; init; } = 30;

    public int RecoveryIntervalSeconds { get; init; } = 30;

    public int ScoreRecalculationIntervalSeconds { get; init; } = 60;
}

