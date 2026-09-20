using System.Text.Json.Serialization;

namespace TradingViewReplica.MarketData.Yahoo.Dto;

// Verified against a live query1.finance.yahoo.com/v1/finance/search response (September 2026).
// The real payload also carries "news", "nav", etc. - intentionally not modeled, we only need quotes.

internal sealed class YahooSearchResponse
{
    [JsonPropertyName("quotes")]
    public List<YahooSearchQuote>? Quotes { get; set; }
}

internal sealed class YahooSearchQuote
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("shortname")]
    public string? ShortName { get; set; }

    [JsonPropertyName("longname")]
    public string? LongName { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("exchDisp")]
    public string? ExchangeDisplay { get; set; }

    [JsonPropertyName("quoteType")]
    public string? QuoteType { get; set; }

    [JsonPropertyName("typeDisp")]
    public string? TypeDisplay { get; set; }
}
