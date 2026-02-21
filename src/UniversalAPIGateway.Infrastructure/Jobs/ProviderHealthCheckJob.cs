using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Jobs;

public sealed class ProviderHealthCheckJob(
    IProviderHealthTracker providerHealthTracker,
    IOptions<ProviderHealthLifecycleOptions> options,
    ILogger<ProviderHealthCheckJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var disabled = await providerHealthTracker.DisableUnhealthyProvidersAsync(stoppingToken);
                if (disabled.Count > 0)
                {
                    logger.LogWarning("Disabled unhealthy providers: {Providers}", string.Join(',', disabled));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "ProviderHealthCheckJob failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, options.Value.HealthCheckIntervalSeconds)), stoppingToken);
        }
    }
}
