using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using UniversalAPIGateway.Infrastructure.Adapters;
using UniversalAPIGateway.Infrastructure.Configuration;
using UniversalAPIGateway.Infrastructure.Policies;

namespace UniversalAPIGateway.Application.Tests;

public sealed class InfrastructureComponentsTests
{
    [Fact]
    public async Task MockProviderAdapter_ReturnsDeterministicPayload()
    {
        var sut = new MockProviderAdapter();

        var response = await sut.ExecuteAsync("hello", CancellationToken.None);

        Assert.Equal("mock", response.ProviderKey);
        Assert.Equal("mock::hello", response.Result);
    }

    [Fact]
    public async Task PortkeyAdapter_ThrowsTimeoutRejectedException_WhenProviderTimesOut()
    {
        var options = Options.Create(new PortkeyOptions
        {
            BaseUrl = "https://unit.test/",
            TimeoutSeconds = 1,
            ApiKey = string.Empty
        });

        var slowHandler = new DelayedResponseHandler(TimeSpan.FromSeconds(2));
        var httpClient = new HttpClient(slowHandler)
        {
            BaseAddress = new Uri(options.Value.BaseUrl)
        };

        var sut = new PortkeyAdapter(httpClient, options, new ProviderResiliencePolicies());

        await Assert.ThrowsAsync<TimeoutRejectedException>(() => sut.ExecuteAsync("payload", CancellationToken.None));
    }

    private sealed class DelayedResponseHandler(TimeSpan delay) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"ok\"}", Encoding.UTF8, "application/json")
            };
        }
    }
}
