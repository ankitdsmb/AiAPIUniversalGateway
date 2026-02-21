using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace UniversalAPIGateway.Infrastructure.Policies;

public interface IProviderResiliencePolicies
{
    ResiliencePipeline CreatePipeline(string providerName, TimeSpan timeout);
}

public sealed class ProviderResiliencePolicies : IProviderResiliencePolicies
{
    public ResiliencePipeline CreatePipeline(string providerName, TimeSpan timeout)
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout,
                OnTimeout = _ => ValueTask.CompletedTask
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(150),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => ex is not OperationCanceledException)
            })
            .Build();
    }
}
