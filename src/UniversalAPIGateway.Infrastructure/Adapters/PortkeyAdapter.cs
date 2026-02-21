using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;

namespace UniversalAPIGateway.Infrastructure.Adapters;

public sealed class PortkeyAdapter : IProviderAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;
    private readonly PortkeyOptions options;
    private readonly ResiliencePipeline resiliencePipeline;

    public PortkeyAdapter(
        HttpClient httpClient,
        IOptions<PortkeyOptions> options,
        IProviderResiliencePolicies providerResiliencePolicies)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        resiliencePipeline = providerResiliencePolicies.CreatePipeline("portkey", TimeSpan.FromSeconds(this.options.TimeoutSeconds));

        if (!string.IsNullOrWhiteSpace(this.options.ApiKey))
        {
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.options.ApiKey);
        }
    }

    public Provider Provider { get; } = new(
        new ProviderKey("portkey"),
        "Portkey",
        ProviderCapability.TextGeneration);

    public async Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        var request = new PortkeyRequest(payload);

        return await resiliencePipeline.ExecuteAsync(async token =>
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "v1/proxy")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json")
            };

            using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var parsed = await JsonSerializer.DeserializeAsync<PortkeyResponse>(stream, JsonOptions, token)
                ?? throw new InvalidOperationException("Portkey response was empty.");

            return new GatewayResponse(Provider.Key.Value, parsed.Result);
        }, cancellationToken);
    }

    private sealed record PortkeyRequest(string Input);

    private sealed record PortkeyResponse(string Result);
}
