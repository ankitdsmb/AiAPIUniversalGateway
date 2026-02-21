using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Tests;

public sealed class DomainModelsTests
{
    [Fact]
    public void QuotaInfo_ComputesRemainingUnits()
    {
        var quota = new QuotaInfo("tenant-a", 100, 35, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));

        Assert.Equal(65, quota.Remaining);
        Assert.True(quota.HasCapacity(60));
        Assert.False(quota.HasCapacity(70));
    }

    [Fact]
    public void Provider_SupportsDeclaredCapability()
    {
        var provider = new Provider(
            new ProviderKey("alpha"),
            "Provider Alpha",
            ProviderCapability.TextGeneration | ProviderCapability.Translation);

        Assert.True(provider.Supports(ProviderCapability.TextGeneration));
        Assert.False(provider.Supports(ProviderCapability.Embeddings));
    }
}
