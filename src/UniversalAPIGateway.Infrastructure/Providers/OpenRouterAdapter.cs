using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class OpenRouterAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "OpenRouter";

    public OpenRouterAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("openrouter"),
        "OpenRouter",
        ProviderCapability.TextGeneration | ProviderCapability.Translation);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { model = Options.Model, messages = new[] { new { role = "user", content = payload } } };
        return new HttpRequestMessage(HttpMethod.Post, "api/v1/chat/completions") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);
        return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}
