using Npgsql;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;

namespace UniversalAPIGateway.Infrastructure.Repositories;

public sealed class PostgreSqlRequestLogRepository(string connectionString) : IRequestLogRepository
{
    public async ValueTask AddAsync(RequestLog requestLog, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO request_logs
                (request_id, occurred_at, provider_key, succeeded, duration_ms, error_code, units_consumed)
            VALUES
                (@request_id, @occurred_at, @provider_key, @succeeded, @duration_ms, @error_code, @units_consumed);
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new("request_id", requestLog.RequestId),
                new("occurred_at", requestLog.OccurredAt.UtcDateTime),
                new("provider_key", requestLog.ProviderKey.Value),
                new("succeeded", requestLog.Succeeded),
                new("duration_ms", requestLog.Duration.TotalMilliseconds),
                new("error_code", requestLog.ErrorCode ?? (object)DBNull.Value),
                new("units_consumed", requestLog.UnitsConsumed)
            }
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
