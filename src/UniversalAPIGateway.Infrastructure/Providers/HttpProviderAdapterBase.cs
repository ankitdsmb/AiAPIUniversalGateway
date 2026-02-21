using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Infrastructure.Providers;

public abstract class HttpProviderAdapterBase : IProviderAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;
    private readonly ProviderEndpointOptions options;
    private readonly IProviderHealthTracker providerHealthTracker;
    private readonly ResiliencePipeline pipeline;

    protected HttpProviderAdapterBase(
        HttpClient httpClient,
        IOptionsMonitor<ProviderEndpointOptions> optionsMonitor,
        string optionsName,
        IProviderResiliencePolicies resiliencePolicies,
        IProviderHealthTracker providerHealthTracker)
    {
        this.httpClient = httpClient;
        options = optionsMonitor.Get(optionsName);
        this.providerHealthTracker = providerHealthTracker;
        pipeline = resiliencePolicies.CreatePipeline(optionsName, TimeSpan.FromSeconds(options.TimeoutSeconds));

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        }
    }

    public abstract Provider Provider { get; }

    protected ProviderEndpointOptions Options => options;

    public async Task<GatewayResponse> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await pipeline.ExecuteAsync(
                async token => await SendAndParseAsync(payload, token),
                cancellationToken);

            await providerHealthTracker.MarkHealthyAsync(Provider.Key.Value, cancellationToken);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (ex is not InvalidOperationException invalidOperationException
                || (!invalidOperationException.Message.Contains("(429)", StringComparison.Ordinal)
                    && !invalidOperationException.Message.Contains("(402)", StringComparison.Ordinal)))
            {
                await providerHealthTracker.MarkTemporaryUnavailableAsync(
                    Provider.Key.Value,
                    "request_failure",
                    TimeSpan.FromSeconds(options.CooldownSeconds),
                    cancellationToken);
            }

            throw;
        }
    }

    protected abstract HttpRequestMessage BuildHttpRequest(string payload);

    protected abstract string ParseProviderResult(string responseBody);

    private async Task<GatewayResponse> SendAndParseAsync(string payload, CancellationToken cancellationToken)
    {
        using var message = BuildHttpRequest(payload);
        using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.PaymentRequired or (HttpStatusCode)429)
            {
                await providerHealthTracker.MarkTemporaryUnavailableAsync(
                    Provider.Key.Value,
                    "quota_exceeded",
                    TimeSpan.FromSeconds(options.CooldownSeconds),
                    cancellationToken);
            }

            throw new InvalidOperationException($"{Provider.DisplayName} request failed ({(int)response.StatusCode}): {body}");
        }

        var parsed = ParseProviderResult(body);
        if (string.IsNullOrWhiteSpace(parsed))
        {
            throw new InvalidOperationException($"{Provider.DisplayName} returned an invalid payload.");
        }

        return new GatewayResponse(Provider.Key.Value, parsed.Trim());
    }

    protected static StringContent JsonContent<T>(T payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
    }

    protected static JsonDocument ParseJson(string json) => JsonDocument.Parse(json);
}
