using Npgsql;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Repositories;

public sealed class PostgreSqlProviderKeyRepository(string connectionString) : IProviderKeyRepository
{
    public async ValueTask<IReadOnlyCollection<ProviderKey>> GetAllEnabledAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT provider_key
            FROM providers
            WHERE is_enabled = TRUE;
            """;

        var results = new List<ProviderKey>();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProviderKey(reader.GetString(0)));
        }

        return results;
    }
}
