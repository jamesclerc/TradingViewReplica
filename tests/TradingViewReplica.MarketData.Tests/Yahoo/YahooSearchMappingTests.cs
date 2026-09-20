using System.Text.Json;
using TradingViewReplica.MarketData.Tests.TestSupport;
using TradingViewReplica.MarketData.Yahoo.Dto;

namespace TradingViewReplica.MarketData.Tests.Yahoo;

public class YahooSearchMappingTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_LiveSearchFixture_PopulatesQuotes()
    {
        var json = FixtureLoader.ReadText("search_apple.json");

        var response = JsonSerializer.Deserialize<YahooSearchResponse>(json, JsonOptions);

        Assert.NotNull(response?.Quotes);
        Assert.Equal(3, response!.Quotes!.Count);

        var apple = response.Quotes[0];
        Assert.Equal("AAPL", apple.Symbol);
        Assert.Equal("Apple Inc.", apple.ShortName);
        Assert.Equal("NASDAQ", apple.ExchangeDisplay);
        Assert.Equal("EQUITY", apple.QuoteType);
    }
}
