namespace UniversalAPIGateway.Application.Abstractions;

public interface IProviderRegistryPersistence
{
    Task UpsertAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken);

    Task<ProviderRegistryEntry?> GetByKeyAsync(string providerKey, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProviderRegistryEntry>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> SetEnabledAsync(string providerKey, bool isEnabled, DateTimeOffset updatedAtUtc, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> DisableStaleAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken);
}

public interface IProviderRegistryCache
{
    Task SetAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken);

    Task RemoveAsync(string providerKey, CancellationToken cancellationToken);
}
