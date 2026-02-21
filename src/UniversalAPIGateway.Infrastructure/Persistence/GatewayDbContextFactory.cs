using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UniversalAPIGateway.Infrastructure.Persistence;

public sealed class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext>
{
    public GatewayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("GATEWAY_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=universal_gateway;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);
        return new GatewayDbContext(optionsBuilder.Options);
    }
}
