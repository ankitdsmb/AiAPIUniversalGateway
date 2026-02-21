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
        await EnsureTableExistsAsync(connection, cancellationToken);

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

    private static async Task EnsureTableExistsAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS request_logs (
                id BIGSERIAL PRIMARY KEY,
                request_id TEXT NOT NULL,
                occurred_at TIMESTAMPTZ NOT NULL,
                provider_key TEXT NOT NULL,
                succeeded BOOLEAN NOT NULL,
                duration_ms DOUBLE PRECISION NOT NULL,
                error_code TEXT NULL,
                units_consumed BIGINT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_request_logs_occurred_at ON request_logs (occurred_at DESC);
            CREATE INDEX IF NOT EXISTS ix_request_logs_provider_key_occurred_at ON request_logs (provider_key, occurred_at DESC);
            CREATE INDEX IF NOT EXISTS ix_request_logs_request_id ON request_logs (request_id);
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
