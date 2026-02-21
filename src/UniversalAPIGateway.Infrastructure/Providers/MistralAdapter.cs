using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class MistralAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "Mistral";

    public MistralAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("mistral"),
        "Mistral AI",
        ProviderCapability.TextGeneration | ProviderCapability.Translation);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { model = Options.Model, messages = new[] { new { role = "user", content = payload } } };
        return new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);

        return document.RootElement.TryGetProperty("choices", out var choices)
            && choices.ValueKind == System.Text.Json.JsonValueKind.Array
            && choices.GetArrayLength() > 0
            && choices[0].TryGetProperty("message", out var message)
            && message.TryGetProperty("content", out var content)
            ? content.GetString() ?? string.Empty
            : string.Empty;
    }
}
