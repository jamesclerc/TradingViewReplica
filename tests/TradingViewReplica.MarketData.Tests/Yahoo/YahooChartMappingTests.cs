using System.Text.Json;
using TradingViewReplica.MarketData.Tests.TestSupport;
using TradingViewReplica.MarketData.Yahoo;
using TradingViewReplica.MarketData.Yahoo.Dto;

namespace TradingViewReplica.MarketData.Tests.Yahoo;

public class YahooChartMappingTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_LiveChartFixture_PopulatesMetaAndSeries()
    {
        var json = FixtureLoader.ReadText("chart_aapl_5d.json");

        var response = JsonSerializer.Deserialize<YahooChartResponse>(json, JsonOptions);
        var result = response?.Chart?.Result?.Single();

        Assert.NotNull(result);
        Assert.Equal("AAPL", result!.Meta?.Symbol);
        Assert.Equal(336.13m, result.Meta?.RegularMarketPrice);
        Assert.Equal(5, result.Timestamp?.Count);
        Assert.Equal(5, result.Indicators?.Quote?.Single().Close?.Count);
    }

    [Fact]
    public void MapCandles_LiveChartFixture_ProducesOneCandlePerTimestamp()
    {
        var result = LoadResult("chart_aapl_5d.json");

        var candles = YahooFinanceMarketDataProvider.MapCandles(result);

        Assert.Equal(5, candles.Count);
        var first = candles[0];
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1789392600), first.Timestamp);
        Assert.Equal(334.7900085449219m, first.Open);
        Assert.Equal(333.0799865722656m, first.Close);
        Assert.Equal(39269100, first.Volume);
    }

    [Fact]
    public void MapCandles_RowWithNullPrices_IsSkippedNotZeroed()
    {
        var result = new YahooChartResult
        {
            Timestamp = [1000, 2000, 3000],
            Indicators = new YahooIndicators
            {
                Quote =
                [
                    new YahooQuoteIndicator
                    {
                        Open = [1m, null, 3m],
                        High = [1m, null, 3m],
                        Low = [1m, null, 3m],
                        Close = [1m, null, 3m],
                        Volume = [10, null, 30]
                    }
                ]
            }
        };

        var candles = YahooFinanceMarketDataProvider.MapCandles(result);

        Assert.Equal(2, candles.Count);
        Assert.DoesNotContain(candles, c => c.Timestamp == DateTimeOffset.FromUnixTimeSeconds(2000));
    }

    [Fact]
    public void MapQuote_LiveChartFixture_ReadsMetaSnapshot()
    {
        var result = LoadResult("chart_aapl_5d.json");

        var quote = YahooFinanceMarketDataProvider.MapQuote(result);

        Assert.Equal("AAPL", quote.Symbol);
        Assert.Equal(336.13m, quote.RegularMarketPrice);
        Assert.Equal(332.27m, quote.PreviousClose);
        Assert.Equal(86241049, quote.RegularMarketVolume);
        Assert.Equal("USD", quote.Currency);
        Assert.Equal("Apple Inc.", quote.DisplayName);
    }

    [Theory]
    [InlineData("chart_error_not_found.json", "Not Found", "No data found, symbol may be delisted")]
    [InlineData(
        "chart_error_unprocessable.json",
        "Unprocessable Entity",
        "1m data not available for startTime=1787219117 and endTime=1789897517. Only 8 days worth of 1m granularity data are allowed to be fetched per request.")]
    public void Deserialize_LiveErrorFixtures_PopulatesErrorEnvelope(
        string fixture, string expectedCode, string expectedDescription)
    {
        var json = FixtureLoader.ReadText(fixture);

        var response = JsonSerializer.Deserialize<YahooChartResponse>(json, JsonOptions);

        Assert.Null(response?.Chart?.Result);
        Assert.Equal(expectedCode, response?.Chart?.Error?.Code);
        Assert.Equal(expectedDescription, response?.Chart?.Error?.Description);
    }

    private static YahooChartResult LoadResult(string fixture)
    {
        var json = FixtureLoader.ReadText(fixture);
        var response = JsonSerializer.Deserialize<YahooChartResponse>(json, JsonOptions);
        return response!.Chart!.Result!.Single();
    }
}
