using Npgsql;
using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Infrastructure.Repositories;

public sealed class PostgreSqlProviderRegistryRepository(string connectionString) : IProviderRegistryPersistence
{
    public async Task UpsertAsync(ProviderRegistryEntry entry, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO provider_registry(provider_key, display_name, endpoint, capabilities, is_enabled, last_heartbeat_utc, updated_at_utc)
            VALUES (@provider_key, @display_name, @endpoint, @capabilities, @is_enabled, @last_heartbeat_utc, @updated_at_utc)
            ON CONFLICT (provider_key)
            DO UPDATE SET
                display_name = EXCLUDED.display_name,
                endpoint = EXCLUDED.endpoint,
                capabilities = EXCLUDED.capabilities,
                is_enabled = EXCLUDED.is_enabled,
                last_heartbeat_utc = EXCLUDED.last_heartbeat_utc,
                updated_at_utc = EXCLUDED.updated_at_utc;
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureTableExistsAsync(connection, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("provider_key", entry.ProviderKey);
        command.Parameters.AddWithValue("display_name", entry.DisplayName);
        command.Parameters.AddWithValue("endpoint", entry.Endpoint);
        command.Parameters.AddWithValue("capabilities", entry.Capabilities);
        command.Parameters.AddWithValue("is_enabled", entry.IsEnabled);
        command.Parameters.AddWithValue("last_heartbeat_utc", entry.LastHeartbeatUtc.UtcDateTime);
        command.Parameters.AddWithValue("updated_at_utc", entry.UpdatedAtUtc.UtcDateTime);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ProviderRegistryEntry?> GetByKeyAsync(string providerKey, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT provider_key, display_name, endpoint, capabilities, is_enabled, last_heartbeat_utc, updated_at_utc
            FROM provider_registry
            WHERE provider_key = @provider_key;
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureTableExistsAsync(connection, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("provider_key", providerKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadEntry(reader);
    }

    public async Task<IReadOnlyCollection<string>> DisableStaleAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE provider_registry
            SET is_enabled = FALSE,
                updated_at_utc = NOW() AT TIME ZONE 'UTC'
            WHERE is_enabled = TRUE
              AND last_heartbeat_utc < @stale_before_utc
            RETURNING provider_key;
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureTableExistsAsync(connection, cancellationToken);

        var disabled = new List<string>();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("stale_before_utc", staleBeforeUtc.UtcDateTime);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            disabled.Add(reader.GetString(0));
        }

        return disabled;
    }

    private static ProviderRegistryEntry ReadEntry(NpgsqlDataReader reader)
    {
        var providerKey = reader.GetString(0);
        var displayName = reader.GetString(1);
        var endpoint = reader.GetString(2);
        var capabilities = reader.GetString(3);
        var isEnabled = reader.GetBoolean(4);
        var lastHeartbeatUtc = DateTime.SpecifyKind(reader.GetDateTime(5), DateTimeKind.Utc);
        var updatedAtUtc = DateTime.SpecifyKind(reader.GetDateTime(6), DateTimeKind.Utc);

        return new ProviderRegistryEntry(
            providerKey,
            displayName,
            endpoint,
            capabilities,
            isEnabled,
            new DateTimeOffset(lastHeartbeatUtc),
            new DateTimeOffset(updatedAtUtc));
    }

    private static async Task EnsureTableExistsAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS provider_registry (
                provider_key TEXT PRIMARY KEY,
                display_name TEXT NOT NULL,
                endpoint TEXT NOT NULL,
                capabilities TEXT NOT NULL,
                is_enabled BOOLEAN NOT NULL,
                last_heartbeat_utc TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                updated_at_utc TIMESTAMP WITHOUT TIME ZONE NOT NULL
            );
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
