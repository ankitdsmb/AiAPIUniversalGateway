using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class AssemblyAIAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "AssemblyAI";

    public AssemblyAIAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("assemblyai"),
        "AssemblyAI",
        ProviderCapability.SpeechToText);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { audio_url = payload };
        return new HttpRequestMessage(HttpMethod.Post, "v2/transcript") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);

        return document.RootElement.TryGetProperty("text", out var text)
            ? text.GetString() ?? string.Empty
            : string.Empty;
    }
}
