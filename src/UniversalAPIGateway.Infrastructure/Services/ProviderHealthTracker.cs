using StackExchange.Redis;

namespace UniversalAPIGateway.Infrastructure.Services;

public interface IProviderHealthTracker
{
    Task MarkTemporaryUnavailableAsync(string providerKey, string reason, TimeSpan cooldown, CancellationToken cancellationToken);

    Task MarkHealthyAsync(string providerKey, CancellationToken cancellationToken);
}

public sealed class ProviderHealthTracker(IConnectionMultiplexer multiplexer) : IProviderHealthTracker
{
    private readonly IDatabase database = multiplexer.GetDatabase();

    public Task MarkTemporaryUnavailableAsync(string providerKey, string reason, TimeSpan cooldown, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return database.StringSetAsync($"provider_health:{providerKey}", reason, cooldown);
    }

    public Task MarkHealthyAsync(string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return database.KeyDeleteAsync($"provider_health:{providerKey}");
    }
}
