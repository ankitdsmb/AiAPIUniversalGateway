using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Jobs;

public sealed class ProviderRecoveryJob(
    IProviderHealthTracker providerHealthTracker,
    IOptions<ProviderHealthLifecycleOptions> options,
    ILogger<ProviderRecoveryJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var recovered = await providerHealthTracker.RecoverProvidersAsync(stoppingToken);
                if (recovered.Count > 0)
                {
                    logger.LogInformation("Recovered providers: {Providers}", string.Join(',', recovered));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "ProviderRecoveryJob failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, options.Value.RecoveryIntervalSeconds)), stoppingToken);
        }
    }
}
