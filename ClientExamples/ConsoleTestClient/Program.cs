using ClientExamples;

await RunAsync();

static async Task RunAsync()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    using var httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7135/")
    };

    var gatewayClient = new GatewayClient(httpClient);

    try
    {
        Console.WriteLine("--- Basic HttpClient sample ---");
        await BasicHttpClientExample.RunAsync(cts.Token);

        Console.WriteLine("\n--- Strongly typed client scenarios ---");
        await ExecuteScenarioAsync("1) Text -> Text", () => GatewayScenarioExamples.TextToTextAsync(gatewayClient, cts.Token));
        await ExecuteScenarioAsync("2) Text -> Image", () => GatewayScenarioExamples.TextToImageAsync(gatewayClient, cts.Token));
        await ExecuteScenarioAsync("3) Audio -> Text", () => GatewayScenarioExamples.AudioToTextAsync(gatewayClient, cts.Token));
        await ExecuteScenarioAsync("4) Any -> Any", () => GatewayScenarioExamples.AnyToAnyAsync(gatewayClient, cts.Token));
    }
    catch (GatewayClientException ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"GatewayClientException: {ex.Message}");
        Console.WriteLine($"Body: {ex.ResponseBody}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unhandled error: {ex.Message}");
        Console.ResetColor();
    }
}

static async Task ExecuteScenarioAsync(string title, Func<Task<GatewayResponse>> scenario)
{
    try
    {
        var response = await scenario();
        Console.WriteLine(title);
        Console.WriteLine(GatewayScenarioExamples.PrettyPrint(response));
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{title} failed: {ex.Message}");
        Console.ResetColor();
    }
}
