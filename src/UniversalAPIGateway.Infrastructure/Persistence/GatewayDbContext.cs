using Microsoft.EntityFrameworkCore;
using UniversalAPIGateway.Infrastructure.Persistence.Entities;

namespace UniversalAPIGateway.Infrastructure.Persistence;

public sealed class GatewayDbContext(DbContextOptions<GatewayDbContext> options) : DbContext(options)
{
    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProviderKeyEntity> ProviderKeys => Set<ProviderKeyEntity>();
    public DbSet<ProviderHealthEntity> ProviderHealth => Set<ProviderHealthEntity>();
    public DbSet<ProviderScoreEntity> ProviderScores => Set<ProviderScoreEntity>();
    public DbSet<GatewayRequestEntity> GatewayRequests => Set<GatewayRequestEntity>();
    public DbSet<GatewayResponseEntity> GatewayResponses => Set<GatewayResponseEntity>();
    public DbSet<ProviderTelemetryEntity> ProviderTelemetry => Set<ProviderTelemetryEntity>();
    public DbSet<ProviderPerformanceEntity> ProviderPerformance => Set<ProviderPerformanceEntity>();
    public DbSet<RoutingDecisionEntity> RoutingDecisions => Set<RoutingDecisionEntity>();
    public DbSet<ProviderFailureEntity> ProviderFailures => Set<ProviderFailureEntity>();
    public DbSet<ProviderCooldownEntity> ProviderCooldowns => Set<ProviderCooldownEntity>();
    public DbSet<ApiClientEntity> ApiClients => Set<ApiClientEntity>();
    public DbSet<ClientUsageStatEntity> ClientUsageStats => Set<ClientUsageStatEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GatewayDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
