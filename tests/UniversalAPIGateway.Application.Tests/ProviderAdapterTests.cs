using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using UniversalAPIGateway.Application.Services;
using UniversalAPIGateway.Domain.Entities;
using UniversalAPIGateway.Domain.Ports;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;
using UniversalAPIGateway.Infrastructure.Providers;
using UniversalAPIGateway.Infrastructure.Services;

namespace UniversalAPIGateway.Application.Tests;

public sealed class ProviderAdapterTests
{
    [Theory]
    [InlineData(typeof(OpenRouterAdapter), "{\"choices\":[{\"message\":{\"content\":\"openrouter-result\"}}]}", "openrouter")]
    [InlineData(typeof(HuggingFaceAdapter), "[{\"generated_text\":\"hf-result\"}]", "huggingface")]
    [InlineData(typeof(TogetherAIAdapter), "{\"choices\":[{\"message\":{\"content\":\"together-result\"}}]}", "together")]
    [InlineData(typeof(GroqAdapter), "{\"choices\":[{\"message\":{\"content\":\"groq-result\"}}]}", "groq")]
    [InlineData(typeof(ReplicateAdapter), "{\"output\":[\"replicate-result\"]}", "replicate")]
    [InlineData(typeof(MistralAdapter), "{\"choices\":[{\"message\":{\"content\":\"mistral-result\"}}]}", "mistral")]
    [InlineData(typeof(AssemblyAIAdapter), "{\"text\":\"assembly-result\"}", "assemblyai")]
    [InlineData(typeof(FireworksAdapter), "{\"choices\":[{\"message\":{\"content\":\"fireworks-result\"}}]}", "fireworks")]
    [InlineData(typeof(CohereAdapter), "{\"message\":{\"content\":[{\"text\":\"cohere-result\"}]}}", "cohere")]
    [InlineData(typeof(GoogleAIStudioAdapter), "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"google-result\"}]}}]}", "googleaistudio")]

    public async Task Adapter_MapsSuccessfulResponse(Type adapterType, string body, string providerKey)
    {
        var handler = new StaticResponseHandler(HttpStatusCode.OK, body);
        var adapter = CreateAdapter(adapterType, handler);

        var response = await adapter.ExecuteAsync("hello", CancellationToken.None);

        Assert.Equal(providerKey, response.ProviderKey);
        Assert.NotEmpty(response.Result);
    }

    [Fact]
    public async Task Adapter_ThrowsForInvalidResponse()
    {
        var adapter = CreateAdapter(typeof(OpenRouterAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{}"));

        await Assert.ThrowsAnyAsync<Exception>(() => adapter.ExecuteAsync("payload", CancellationToken.None));
    }

    [Fact]
    public async Task Adapter_ThrowsTimeoutRejectedException_OnTimeout()
    {
        var optionsMonitor = new NamedProviderOptionsMonitor(timeoutSeconds: 1);
        var handler = new DelayedResponseHandler(TimeSpan.FromSeconds(2));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://unit.test/") };
        var adapter = new OpenRouterAdapter(httpClient, optionsMonitor, new ProviderResiliencePolicies(), new InMemoryProviderHealthTracker());

        await Assert.ThrowsAsync<TimeoutRejectedException>(() => adapter.ExecuteAsync("payload", CancellationToken.None));
    }

    [Fact]
    public async Task Adapter_NormalizesErrors_AndMarksQuotaExceededAsUnavailable()
    {
        var healthTracker = new InMemoryProviderHealthTracker();
        var adapter = CreateAdapter(typeof(OpenRouterAdapter), new StaticResponseHandler((HttpStatusCode)429, "quota exceeded"), healthTracker);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.ExecuteAsync("payload", CancellationToken.None));

        Assert.Contains("OpenRouter", ex.Message);
        Assert.Equal("quota_exceeded", healthTracker.GetState("openrouter"));
    }

    [Fact]
    public void Adapters_ReportCapabilities_ForRouting()
    {
        var providers = new IProviderAdapter[]
        {
            CreateAdapter(typeof(OpenRouterAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"content\":\"ok\"}}]}")),
            CreateAdapter(typeof(HuggingFaceAdapter), new StaticResponseHandler(HttpStatusCode.OK, "[{\"generated_text\":\"ok\"}]")),
            CreateAdapter(typeof(TogetherAIAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"content\":\"ok\"}}]}")),
            CreateAdapter(typeof(GroqAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"content\":\"ok\"}}]}")),
            CreateAdapter(typeof(ReplicateAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"output\":\"ok\"}")),
            CreateAdapter(typeof(MistralAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"content\":\"ok\"}}]}")),
            CreateAdapter(typeof(AssemblyAIAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"text\":\"ok\"}")),
            CreateAdapter(typeof(FireworksAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"content\":\"ok\"}}]}")),
            CreateAdapter(typeof(CohereAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"message\":{\"content\":[{\"text\":\"ok\"}]}}")),
            CreateAdapter(typeof(GoogleAIStudioAdapter), new StaticResponseHandler(HttpStatusCode.OK, "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]}}]}"))
        };

        Assert.All(providers.Where(a => a.Provider.Key.Value != "assemblyai"), adapter => Assert.True(adapter.Provider.Supports(ProviderCapability.TextGeneration)));
        Assert.True(providers.Single(a => a.Provider.Key.Value == "assemblyai").Provider.Supports(ProviderCapability.SpeechToText));
    }

    [Fact]
    public async Task Integration_FallbackMovesToNextProvider_WhenNewProviderFails()
    {
        var failing = CreateAdapter(typeof(OpenRouterAdapter), new StaticResponseHandler(HttpStatusCode.InternalServerError, "oops"));
        var fallback = new EchoProviderAdapter();
        var strategy = new DefaultProviderSelectionStrategy();
        var selectionEngine = new ProviderSelectionEngine(strategy);
        var fallbackHandler = new FallbackHandler(selectionEngine);
        var responseNormalizer = new ResponseNormalizer();
        var orchestrator = new OrchestratorService(new[] { failing, fallback }, selectionEngine, fallbackHandler, responseNormalizer);

        var response = await orchestrator.RouteAsync(new GatewayRequest(new ProviderKey("openrouter"), "resilient"), CancellationToken.None);

        Assert.Equal("echo", response.ProviderKey);
        Assert.Equal("resilient", response.Result);
    }

    private static IProviderAdapter CreateAdapter(Type adapterType, HttpMessageHandler handler, InMemoryProviderHealthTracker? healthTracker = null)
    {
        var optionsMonitor = new NamedProviderOptionsMonitor();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://unit.test/") };
        var resilience = new ProviderResiliencePolicies();
        healthTracker ??= new InMemoryProviderHealthTracker();

        return (IProviderAdapter)Activator.CreateInstance(adapterType, httpClient, optionsMonitor, resilience, healthTracker)!;
    }

    private sealed class StaticResponseHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }

    private sealed class DelayedResponseHandler(TimeSpan delay) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"choices\":[{\"message\":{\"content\":\"ok\"}}]}", Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class NamedProviderOptionsMonitor(int timeoutSeconds = 10) : IOptionsMonitor<ProviderEndpointOptions>
    {
        public ProviderEndpointOptions CurrentValue => Get(OpenRouterAdapter.OptionsName);

        public ProviderEndpointOptions Get(string? name) => new()
        {
            BaseUrl = "https://unit.test/",
            ApiKey = string.Empty,
            Model = name switch
            {
                var n when n == ReplicateAdapter.OptionsName => "owner/model",
                var n when n == GoogleAIStudioAdapter.OptionsName => "gemini-1.5-flash",
                _ => "unit-model"
            },
            TimeoutSeconds = timeoutSeconds,
            CooldownSeconds = 30
        };

        public IDisposable? OnChange(Action<ProviderEndpointOptions, string?> listener) => null;
    }

    private sealed class InMemoryProviderHealthTracker : IProviderHealthTracker
    {
        private readonly Dictionary<string, string> states = new(StringComparer.OrdinalIgnoreCase);

        public Task MarkTemporaryUnavailableAsync(string providerKey, string reason, TimeSpan cooldown, CancellationToken cancellationToken)
        {
            states[providerKey] = reason;
            return Task.CompletedTask;
        }

        public Task MarkHealthyAsync(string providerKey, CancellationToken cancellationToken)
        {
            states.Remove(providerKey);
            return Task.CompletedTask;
        }

        public string? GetState(string providerKey) => states.GetValueOrDefault(providerKey);
    }
}
