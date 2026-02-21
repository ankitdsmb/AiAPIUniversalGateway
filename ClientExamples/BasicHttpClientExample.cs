using System.Net.Http.Json;
using System.Text.Json;

namespace ClientExamples;

public static class BasicHttpClientExample
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7135/")
        };

        var request = new ExecuteAiRequest(
            ProviderKey: "echo",
            Payload: "Hello from BasicHttpClientExample");

        using var response = await httpClient.PostAsJsonAsync("v1/ai/execute", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Gateway request failed ({(int)response.StatusCode} {response.ReasonPhrase}). Body: {errorBody}");
        }

        var gatewayResponse = await response.Content.ReadFromJsonAsync<ExecuteAiResponse>(SerializerOptions, cancellationToken)
            ?? throw new InvalidOperationException("Gateway returned an empty response body.");

        Console.WriteLine("Basic response:");
        Console.WriteLine(JsonSerializer.Serialize(gatewayResponse, SerializerOptions));
    }

    public sealed record ExecuteAiRequest(string ProviderKey, string Payload);

    public sealed record ExecuteAiResponse(string ProviderKey, string Result);
}
