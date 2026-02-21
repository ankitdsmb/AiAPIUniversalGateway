namespace UniversalAPIGateway.Domain.Entities;

public enum ProviderHealthStatus
{
    Healthy = 0,
    Degraded = 1,
    RateLimited = 2,
    QuotaExceeded = 3,
    Disabled = 4
}

