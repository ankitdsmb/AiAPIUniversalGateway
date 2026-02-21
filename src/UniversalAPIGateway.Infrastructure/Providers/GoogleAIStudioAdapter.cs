using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class GoogleAIStudioAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "GoogleAIStudio";

    public GoogleAIStudioAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("googleaistudio"),
        "Google AI Studio",
        ProviderCapability.TextGeneration);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var endpoint = $"v1beta/models/{Options.Model}:generateContent?key={Uri.EscapeDataString(Options.ApiKey)}";
        var request = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = payload } }
                }
            }
        };

        return new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);

        return document.RootElement.TryGetProperty("candidates", out var candidates)
            && candidates.ValueKind == System.Text.Json.JsonValueKind.Array
            && candidates.GetArrayLength() > 0
            && candidates[0].TryGetProperty("content", out var content)
            && content.TryGetProperty("parts", out var parts)
            && parts.ValueKind == System.Text.Json.JsonValueKind.Array
            && parts.GetArrayLength() > 0
            && parts[0].TryGetProperty("text", out var text)
            ? text.GetString() ?? string.Empty
            : string.Empty;
    }
}
