using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalAPIGateway.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LatencyMs",
                table: "ProviderTelemetry",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "ProviderTelemetry",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Tokens",
                table: "ProviderTelemetry",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdated",
                table: "ProviderScores",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "AvgLatency",
                table: "ProviderPerformance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ProviderPerformance",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "QuotaLimit",
                table: "ProviderKeys",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "QuotaUsed",
                table: "ProviderKeys",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "AvgLatency",
                table: "ProviderHealth",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailureCount",
                table: "ProviderHealth",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LatencyMs",
                table: "GatewayResponses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "GatewayResponses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "TokenUsage",
                table: "GatewayResponses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RequestsCount",
                table: "ClientUsageStats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ApiClients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "RateLimit",
                table: "ApiClients",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatencyMs",
                table: "ProviderTelemetry");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "ProviderTelemetry");

            migrationBuilder.DropColumn(
                name: "Tokens",
                table: "ProviderTelemetry");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "ProviderScores");

            migrationBuilder.DropColumn(
                name: "AvgLatency",
                table: "ProviderPerformance");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProviderPerformance");

            migrationBuilder.DropColumn(
                name: "QuotaLimit",
                table: "ProviderKeys");

            migrationBuilder.DropColumn(
                name: "QuotaUsed",
                table: "ProviderKeys");

            migrationBuilder.DropColumn(
                name: "AvgLatency",
                table: "ProviderHealth");

            migrationBuilder.DropColumn(
                name: "FailureCount",
                table: "ProviderHealth");

            migrationBuilder.DropColumn(
                name: "LatencyMs",
                table: "GatewayResponses");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "GatewayResponses");

            migrationBuilder.DropColumn(
                name: "TokenUsage",
                table: "GatewayResponses");

            migrationBuilder.DropColumn(
                name: "RequestsCount",
                table: "ClientUsageStats");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "RateLimit",
                table: "ApiClients");
        }
    }
}
