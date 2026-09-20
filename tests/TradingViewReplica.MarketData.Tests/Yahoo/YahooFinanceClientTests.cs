using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using TradingViewReplica.MarketData.Tests.TestSupport;
using TradingViewReplica.MarketData.Yahoo;

namespace TradingViewReplica.MarketData.Tests.Yahoo;

public class YahooFinanceClientTests
{
    [Fact]
    public async Task GetChartAsync_FirstAttemptUnauthorized_RefreshesSessionAndRetriesOnce()
    {
        var chartFixture = FixtureLoader.ReadText("chart_aapl_5d.json");
        var chartCallCount = 0;

        HttpResponseMessage Responder(HttpRequestMessage request)
        {
            var uri = request.RequestUri!;

            if (uri.Host == "fc.yahoo.com")
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
                response.Headers.Add("Set-Cookie", "A3=test-cookie-value; Path=/; Domain=.yahoo.com");
                return response;
            }

            if (uri.AbsolutePath.Contains("getcrumb"))
            {
                Assert.Contains("A3=test-cookie-value", request.Headers.GetValues("Cookie"));
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("test-crumb-123") };
            }

            if (uri.AbsolutePath.StartsWith("/v8/finance/chart/"))
            {
                chartCallCount++;
                if (chartCallCount == 1)
                {
                    // First attempt goes out anonymous and gets rejected, as Yahoo does when a
                    // session is required.
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                // Retry must carry the session the 401 triggered YahooSessionManager to obtain.
                Assert.Contains("A3=test-cookie-value", request.Headers.GetValues("Cookie"));
                Assert.Contains("crumb=test-crumb-123", uri.Query);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(chartFixture) };
            }

            throw new InvalidOperationException($"Unexpected request to {uri}");
        }

        await using var provider = BuildServiceProvider(Responder);
        var client = provider.GetRequiredService<YahooFinanceClient>();

        var result = await client.GetChartAsync("AAPL", "1d", "5d", CancellationToken.None);

        Assert.Equal("AAPL", result.Meta?.Symbol);
        Assert.Equal(2, chartCallCount);
    }

    [Fact]
    public async Task GetChartAsync_SymbolNotFound_ThrowsWithSymbolNotFoundErrorType()
    {
        var errorFixture = FixtureLoader.ReadText("chart_error_not_found.json");

        await using var provider = BuildServiceProvider(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(errorFixture) });
        var client = provider.GetRequiredService<YahooFinanceClient>();

        var exception = await Assert.ThrowsAsync<MarketDataProviderException>(
            () => client.GetChartAsync("ZZZZINVALIDXYZ", "1d", "5d", CancellationToken.None));

        Assert.Equal(MarketDataErrorType.SymbolNotFound, exception.ErrorType);
        Assert.Equal("No data found, symbol may be delisted", exception.Message);
    }

    private static ServiceProvider BuildServiceProvider(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var services = new ServiceCollection();

        services.AddHttpClient(YahooClientOptions.FinanceClientName, client =>
                client.BaseAddress = new Uri("https://query1.finance.yahoo.com"))
            .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler(responder));
        services.AddHttpClient(YahooClientOptions.SessionClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new FakeHttpMessageHandler(responder));

        services.Configure<YahooClientOptions>(o => o.BaseUrl = "https://query1.finance.yahoo.com");

        services.AddSingleton<RateLimiter>(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            TokensPerPeriod = 100,
            ReplenishmentPeriod = TimeSpan.FromMilliseconds(1),
            AutoReplenishment = true
        }));
        services.AddSingleton<YahooSessionManager>();
        services.AddSingleton<YahooFinanceClient>();

        return services.BuildServiceProvider();
    }
}
