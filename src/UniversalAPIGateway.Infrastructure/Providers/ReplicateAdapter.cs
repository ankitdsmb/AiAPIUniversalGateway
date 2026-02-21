using Microsoft.Extensions.Options;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public sealed class ReplicateAdapter : HttpProviderAdapterBase
{
    public const string OptionsName = "Replicate";

    public ReplicateAdapter(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
        : base(httpClient, optionsMonitor, OptionsName, resiliencePolicies, providerHealthTracker)
    {
    }

    public override Provider Provider { get; } = new(
        new ProviderKey("replicate"),
        "Replicate",
        ProviderCapability.TextGeneration | ProviderCapability.Translation);

    protected override HttpRequestMessage BuildHttpRequest(string payload)
    {
        var request = new { input = new { prompt = payload } };
        return new HttpRequestMessage(HttpMethod.Post, $"v1/models/{Options.Model}/predictions") { Content = JsonContent(request) };
    }

    protected override string ParseProviderResult(string responseBody)
    {
        using var document = ParseJson(responseBody);
        var output = document.RootElement.GetProperty("output");

        return output.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Array when output.GetArrayLength() > 0 => output[0].GetString() ?? string.Empty,
            System.Text.Json.JsonValueKind.String => output.GetString() ?? string.Empty,
            _ => string.Empty
        };
    }
}
