using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class FireworksAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "Fireworks";

    public FireworksAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("fireworks"),
        "Fireworks AI",
        ProviderCapability.TextGeneration);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { model = Options.Model, messages = new[] { new { role = "user", content = payload } } };
        return new HttpRequestMessage(HttpMethod.Post, "inference/v1/chat/completions") { Content = JsonContent(request) };
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
