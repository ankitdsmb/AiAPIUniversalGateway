using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class HuggingFaceAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "HuggingFace";

    public HuggingFaceAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("huggingface"),
        "Hugging Face",
        ProviderCapability.TextGeneration | ProviderCapability.Embeddings);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { inputs = payload };
        return new HttpRequestMessage(HttpMethod.Post, $"models/{Options.Model}") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);
        var root = document.RootElement;

        if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
        {
            return root[0].GetProperty("generated_text").GetString() ?? string.Empty;
        }

        return root.GetProperty("generated_text").GetString() ?? string.Empty;
    }
}
