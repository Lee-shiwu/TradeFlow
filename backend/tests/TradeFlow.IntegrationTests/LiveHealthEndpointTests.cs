using System.Net;
using System.Net.Http.Json;

namespace TradeFlow.IntegrationTests;

public sealed class LiveHealthEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetLiveHealth_ReturnsHealthyServiceResponse()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/live");
        HealthResponse? body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
        Assert.Equal("TradeFlow.Api", body.Service);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    private sealed record HealthResponse(
        string Status,
        string Service,
        string TraceId);
}
