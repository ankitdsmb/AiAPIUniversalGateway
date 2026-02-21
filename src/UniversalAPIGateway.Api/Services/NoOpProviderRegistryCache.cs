using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Api.Services;

public sealed class NoOpProviderRegistryCache : IProviderRegistryCache
{
    public Task SetAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
