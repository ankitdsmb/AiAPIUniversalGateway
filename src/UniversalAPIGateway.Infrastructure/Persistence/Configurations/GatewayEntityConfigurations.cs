using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversalAPIGateway.Infrastructure.Persistence.Entities;

namespace UniversalAPIGateway.Infrastructure.Persistence.Configurations;

public sealed class ProviderEntityConfiguration : IEntityTypeConfiguration<ProviderEntity>
{
    public void Configure(EntityTypeBuilder<ProviderEntity> builder)
    {
        builder.ToTable("Providers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Endpoint).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Capabilities).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

public sealed class ProviderKeyEntityConfiguration : IEntityTypeConfiguration<ProviderKeyEntity>
{
    public void Configure(EntityTypeBuilder<ProviderKeyEntity> builder)
    {
        builder.ToTable("ProviderKeys");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApiKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ResetPeriod).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => new { x.ProviderId, x.IsActive });

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.ProviderKeys)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProviderHealthEntityConfiguration : IEntityTypeConfiguration<ProviderHealthEntity>
{
    public void Configure(EntityTypeBuilder<ProviderHealthEntity> builder)
    {
        builder.ToTable("ProviderHealth");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SuccessRate).HasPrecision(5, 2);

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.LastChecked);

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.HealthChecks)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProviderScoreEntityConfiguration : IEntityTypeConfiguration<ProviderScoreEntity>
{
    public void Configure(EntityTypeBuilder<ProviderScoreEntity> builder)
    {
        builder.ToTable("ProviderScores");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TaskType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Score).HasPrecision(8, 4);

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.TaskType);
        builder.HasIndex(x => new { x.ProviderId, x.TaskType }).IsUnique();

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Scores)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class GatewayRequestEntityConfiguration : IEntityTypeConfiguration<GatewayRequestEntity>
{
    public void Configure(EntityTypeBuilder<GatewayRequestEntity> builder)
    {
        builder.ToTable("GatewayRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InputType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OutputType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.RequestId).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
    }
}

public sealed class GatewayResponseEntityConfiguration : IEntityTypeConfiguration<GatewayResponseEntity>
{
    public void Configure(EntityTypeBuilder<GatewayResponseEntity> builder)
    {
        builder.ToTable("GatewayResponses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ErrorType).HasMaxLength(200);

        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Request)
            .WithMany(x => x.Responses)
            .HasForeignKey(x => x.RequestId)
            .HasPrincipalKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.GatewayResponses)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProviderTelemetryEntityConfiguration : IEntityTypeConfiguration<ProviderTelemetryEntity>
{
    public void Configure(EntityTypeBuilder<ProviderTelemetryEntity> builder)
    {
        builder.ToTable("ProviderTelemetry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TaskType).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.TaskType);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.ProviderId, x.TaskType, x.Timestamp });

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Telemetry)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProviderPerformanceEntityConfiguration : IEntityTypeConfiguration<ProviderPerformanceEntity>
{
    public void Configure(EntityTypeBuilder<ProviderPerformanceEntity> builder)
    {
        builder.ToTable("ProviderPerformance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TaskType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SuccessRate).HasPrecision(5, 2);
        builder.Property(x => x.QualityScore).HasPrecision(8, 4);

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.TaskType);
        builder.HasIndex(x => new { x.ProviderId, x.TaskType }).IsUnique();

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Performances)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RoutingDecisionEntityConfiguration : IEntityTypeConfiguration<RoutingDecisionEntity>
{
    public void Configure(EntityTypeBuilder<RoutingDecisionEntity> builder)
    {
        builder.ToTable("RoutingDecisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CandidateProviders).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.DecisionScore).HasPrecision(8, 4);

        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.ChosenProviderId);
        builder.HasIndex(x => x.Timestamp);

        builder.HasOne(x => x.Request)
            .WithMany(x => x.RoutingDecisions)
            .HasForeignKey(x => x.RequestId)
            .HasPrincipalKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChosenProvider)
            .WithMany(x => x.RoutingDecisions)
            .HasForeignKey(x => x.ChosenProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProviderFailureEntityConfiguration : IEntityTypeConfiguration<ProviderFailureEntity>
{
    public void Configure(EntityTypeBuilder<ProviderFailureEntity> builder)
    {
        builder.ToTable("ProviderFailures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ErrorType).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.OccurredAt);

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Failures)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProviderCooldownEntityConfiguration : IEntityTypeConfiguration<ProviderCooldownEntity>
{
    public void Configure(EntityTypeBuilder<ProviderCooldownEntity> builder)
    {
        builder.ToTable("ProviderCooldowns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();

        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.DisabledUntil);

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Cooldowns)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ApiClientEntityConfiguration : IEntityTypeConfiguration<ApiClientEntity>
{
    public void Configure(EntityTypeBuilder<ApiClientEntity> builder)
    {
        builder.ToTable("ApiClients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ApiKey).HasMaxLength(500).IsRequired();

        builder.HasIndex(x => x.ClientName).IsUnique();
        builder.HasIndex(x => x.ApiKey).IsUnique();
    }
}

public sealed class ClientUsageStatEntityConfiguration : IEntityTypeConfiguration<ClientUsageStatEntity>
{
    public void Configure(EntityTypeBuilder<ClientUsageStatEntity> builder)
    {
        builder.ToTable("ClientUsageStats");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => new { x.ClientId, x.PeriodStart, x.PeriodEnd }).IsUnique();

        builder.HasOne(x => x.Client)
            .WithMany(x => x.UsageStats)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
