using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Configuration;

namespace UniversalAPIGateway.Infrastructure.Jobs;

public sealed class ScoreRecalculationJob(
    IProviderRegistryPersistence registryPersistence,
    IProviderScoringService providerScoringService,
    IOptions<ProviderHealthLifecycleOptions> options,
    ILogger<ScoreRecalculationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var providers = await registryPersistence.GetAllAsync(stoppingToken);
                foreach (var provider in providers.Where(static x => x.IsEnabled))
                {
                    var capability = ParseCapability(provider.Capabilities);
                    _ = await providerScoringService.ScoreAsync(
                        new Provider(new ProviderKey(provider.ProviderKey), provider.DisplayName, capability, provider.IsEnabled),
                        capability,
                        stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "ScoreRecalculationJob failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, options.Value.ScoreRecalculationIntervalSeconds)), stoppingToken);
        }
    }

    private static ProviderCapability ParseCapability(string capabilities)
    {
        var result = ProviderCapability.None;
        foreach (var item in capabilities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<ProviderCapability>(item, true, out var capability))
            {
                result |= capability;
            }
        }

        return result == ProviderCapability.None ? ProviderCapability.TextGeneration : result;
    }
}
