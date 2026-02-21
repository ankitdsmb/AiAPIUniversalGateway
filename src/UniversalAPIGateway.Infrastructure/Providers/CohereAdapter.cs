using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class CohereAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "Cohere";

    public CohereAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("cohere"),
        "Cohere",
        ProviderCapability.TextGeneration | ProviderCapability.Embeddings);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { model = Options.Model, message = payload };
        return new HttpRequestMessage(HttpMethod.Post, "v2/chat") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);

        return document.RootElement.TryGetProperty("message", out var message)
            && message.TryGetProperty("content", out var content)
            && content.ValueKind == System.Text.Json.JsonValueKind.Array
            && content.GetArrayLength() > 0
            && content[0].TryGetProperty("text", out var text)
            ? text.GetString() ?? string.Empty
            : string.Empty;
    }
}
