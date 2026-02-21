using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalAPIGateway.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApiClients",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                RateLimit = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_ApiClients", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "GatewayRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RequestId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                InputType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                OutputType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_GatewayRequests", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Providers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Endpoint = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Capabilities = table.Column<string>(type: "jsonb", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Providers", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "ClientUsageStats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                RequestsCount = table.Column<long>(type: "bigint", nullable: false),
                PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClientUsageStats", x => x.Id);
                table.ForeignKey("FK_ClientUsageStats_ApiClients_ClientId", x => x.ClientId, "ApiClients", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "GatewayResponses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RequestId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                Success = table.Column<bool>(type: "boolean", nullable: false),
                LatencyMs = table.Column<int>(type: "integer", nullable: false),
                TokenUsage = table.Column<long>(type: "bigint", nullable: false),
                ErrorType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GatewayResponses", x => x.Id);
                table.ForeignKey("FK_GatewayResponses_GatewayRequests_RequestId", x => x.RequestId, "GatewayRequests", "RequestId", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_GatewayResponses_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProviderCooldowns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                DisabledUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderCooldowns", x => x.Id);
                table.ForeignKey("FK_ProviderCooldowns_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProviderFailures",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                ErrorType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderFailures", x => x.Id);
                table.ForeignKey("FK_ProviderFailures_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProviderHealth",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SuccessRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                AvgLatency = table.Column<int>(type: "integer", nullable: false),
                FailureCount = table.Column<int>(type: "integer", nullable: false),
                LastChecked = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderHealth", x => x.Id);
                table.ForeignKey("FK_ProviderHealth_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProviderKeys",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                QuotaLimit = table.Column<long>(type: "bigint", nullable: false),
                QuotaUsed = table.Column<long>(type: "bigint", nullable: false),
                ResetPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderKeys", x => x.Id);
                table.ForeignKey("FK_ProviderKeys_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProviderPerformance",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                TaskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                SuccessRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                AvgLatency = table.Column<int>(type: "integer", nullable: false),
                QualityScore = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderPerformance", x => x.Id);
                table.ForeignKey("FK_ProviderPerformance_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProviderScores",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                TaskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Score = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderScores", x => x.Id);
                table.ForeignKey("FK_ProviderScores_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProviderTelemetry",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                TaskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Success = table.Column<bool>(type: "boolean", nullable: false),
                LatencyMs = table.Column<int>(type: "integer", nullable: false),
                Tokens = table.Column<long>(type: "bigint", nullable: false),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProviderTelemetry", x => x.Id);
                table.ForeignKey("FK_ProviderTelemetry_Providers_ProviderId", x => x.ProviderId, "Providers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RoutingDecisions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RequestId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ChosenProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                CandidateProviders = table.Column<string>(type: "jsonb", nullable: false),
                DecisionScore = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoutingDecisions", x => x.Id);
                table.ForeignKey("FK_RoutingDecisions_GatewayRequests_RequestId", x => x.RequestId, "GatewayRequests", "RequestId", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RoutingDecisions_Providers_ChosenProviderId", x => x.ChosenProviderId, "Providers", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_ApiClients_ApiKey", table: "ApiClients", column: "ApiKey", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ApiClients_ClientName", table: "ApiClients", column: "ClientName", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ClientUsageStats_ClientId", table: "ClientUsageStats", column: "ClientId");
        migrationBuilder.CreateIndex(name: "IX_ClientUsageStats_ClientId_PeriodStart_PeriodEnd", table: "ClientUsageStats", columns: new[] { "ClientId", "PeriodStart", "PeriodEnd" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_GatewayRequests_CreatedAt", table: "GatewayRequests", column: "CreatedAt");
        migrationBuilder.CreateIndex(name: "IX_GatewayRequests_RequestId", table: "GatewayRequests", column: "RequestId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_GatewayResponses_CreatedAt", table: "GatewayResponses", column: "CreatedAt");
        migrationBuilder.CreateIndex(name: "IX_GatewayResponses_ProviderId", table: "GatewayResponses", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_GatewayResponses_RequestId", table: "GatewayResponses", column: "RequestId");
        migrationBuilder.CreateIndex(name: "IX_ProviderCooldowns_DisabledUntil", table: "ProviderCooldowns", column: "DisabledUntil");
        migrationBuilder.CreateIndex(name: "IX_ProviderCooldowns_ProviderId", table: "ProviderCooldowns", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderFailures_OccurredAt", table: "ProviderFailures", column: "OccurredAt");
        migrationBuilder.CreateIndex(name: "IX_ProviderFailures_ProviderId", table: "ProviderFailures", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderHealth_LastChecked", table: "ProviderHealth", column: "LastChecked");
        migrationBuilder.CreateIndex(name: "IX_ProviderHealth_ProviderId", table: "ProviderHealth", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderKeys_ProviderId", table: "ProviderKeys", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderKeys_ProviderId_IsActive", table: "ProviderKeys", columns: new[] { "ProviderId", "IsActive" });
        migrationBuilder.CreateIndex(name: "IX_ProviderPerformance_ProviderId", table: "ProviderPerformance", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderPerformance_ProviderId_TaskType", table: "ProviderPerformance", columns: new[] { "ProviderId", "TaskType" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_ProviderPerformance_TaskType", table: "ProviderPerformance", column: "TaskType");
        migrationBuilder.CreateIndex(name: "IX_Providers_IsActive", table: "Providers", column: "IsActive");
        migrationBuilder.CreateIndex(name: "IX_Providers_Name", table: "Providers", column: "Name", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ProviderScores_ProviderId", table: "ProviderScores", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderScores_ProviderId_TaskType", table: "ProviderScores", columns: new[] { "ProviderId", "TaskType" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_ProviderScores_TaskType", table: "ProviderScores", column: "TaskType");
        migrationBuilder.CreateIndex(name: "IX_ProviderTelemetry_ProviderId", table: "ProviderTelemetry", column: "ProviderId");
        migrationBuilder.CreateIndex(name: "IX_ProviderTelemetry_ProviderId_TaskType_Timestamp", table: "ProviderTelemetry", columns: new[] { "ProviderId", "TaskType", "Timestamp" });
        migrationBuilder.CreateIndex(name: "IX_ProviderTelemetry_TaskType", table: "ProviderTelemetry", column: "TaskType");
        migrationBuilder.CreateIndex(name: "IX_ProviderTelemetry_Timestamp", table: "ProviderTelemetry", column: "Timestamp");
        migrationBuilder.CreateIndex(name: "IX_RoutingDecisions_ChosenProviderId", table: "RoutingDecisions", column: "ChosenProviderId");
        migrationBuilder.CreateIndex(name: "IX_RoutingDecisions_RequestId", table: "RoutingDecisions", column: "RequestId");
        migrationBuilder.CreateIndex(name: "IX_RoutingDecisions_Timestamp", table: "RoutingDecisions", column: "Timestamp");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ClientUsageStats");
        migrationBuilder.DropTable(name: "GatewayResponses");
        migrationBuilder.DropTable(name: "ProviderCooldowns");
        migrationBuilder.DropTable(name: "ProviderFailures");
        migrationBuilder.DropTable(name: "ProviderHealth");
        migrationBuilder.DropTable(name: "ProviderKeys");
        migrationBuilder.DropTable(name: "ProviderPerformance");
        migrationBuilder.DropTable(name: "ProviderScores");
        migrationBuilder.DropTable(name: "ProviderTelemetry");
        migrationBuilder.DropTable(name: "RoutingDecisions");
        migrationBuilder.DropTable(name: "ApiClients");
        migrationBuilder.DropTable(name: "GatewayRequests");
        migrationBuilder.DropTable(name: "Providers");
    }
}
