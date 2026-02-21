using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class GroqAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "Groq";

    public GroqAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("groq"),
        "Groq",
        ProviderCapability.TextGeneration | ProviderCapability.SpeechToText);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { model = Options.Model, messages = new[] { new { role = "user", content = payload } } };
        return new HttpRequestMessage(HttpMethod.Post, "openai/v1/chat/completions") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);
        return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}
