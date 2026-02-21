using System.Net.Http.Json;
using System.Text.Json;

namespace ClientExamples;

public sealed class GatewayClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public GatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://localhost:7135/");
    }

    public Task<GatewayResponse> SendTextAsync(string providerKey, string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return ExecuteAsync(new GatewayRequest(providerKey, text), cancellationToken);
    }

    public async Task<GatewayResponse> ExecuteAsync(GatewayRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("v1/ai/execute", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new GatewayClientException(response.StatusCode, response.ReasonPhrase, errorContent);
        }

        var payload = await response.Content.ReadFromJsonAsync<GatewayResponse>(SerializerOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException("Gateway returned an empty response.");
    }
}

public sealed record GatewayRequest(string ProviderKey, string Payload);

public sealed record GatewayResponse(string ProviderKey, string Result);

public sealed class GatewayClientException(
    System.Net.HttpStatusCode statusCode,
    string? reasonPhrase,
    string responseBody) : Exception($"Gateway call failed ({(int)statusCode} {reasonPhrase}).")
{
    public System.Net.HttpStatusCode StatusCode { get; } = statusCode;

    public string? ReasonPhrase { get; } = reasonPhrase;

    public string ResponseBody { get; } = responseBody;
}

public static class GatewayScenarioExamples
{
    private static readonly JsonSerializerOptions ScenarioSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static Task<GatewayResponse> TextToTextAsync(GatewayClient client, CancellationToken cancellationToken = default)
        => client.SendTextAsync("echo", "Summarize: .NET 8 improves cloud-native performance.", cancellationToken);

    public static Task<GatewayResponse> TextToImageAsync(GatewayClient client, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            task = "text-to-image",
            prompt = "A futuristic API gateway control room in isometric style",
            size = "1024x1024"
        });

        return client.ExecuteAsync(new GatewayRequest("replicate", payload), cancellationToken);
    }

    public static Task<GatewayResponse> AudioToTextAsync(GatewayClient client, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            task = "audio-to-text",
            audioBase64 = "<base64-audio-content>",
            language = "en"
        });

        return client.ExecuteAsync(new GatewayRequest("assemblyai", payload), cancellationToken);
    }

    public static Task<GatewayResponse> AnyToAnyAsync(GatewayClient client, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            task = "any-to-any",
            input = new
            {
                type = "text",
                value = "Create a short caption for an image about AI observability"
            },
            output = new
            {
                type = "text"
            },
            metadata = new
            {
                priority = "normal",
                traceId = Guid.NewGuid().ToString("N")
            }
        });

        return client.ExecuteAsync(new GatewayRequest("openrouter", payload), cancellationToken);
    }

    public static string PrettyPrint(GatewayResponse response)
        => JsonSerializer.Serialize(response, ScenarioSerializerOptions);
}
