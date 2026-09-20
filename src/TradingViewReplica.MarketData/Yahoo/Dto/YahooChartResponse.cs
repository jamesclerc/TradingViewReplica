using System.Text.Json.Serialization;

namespace TradingViewReplica.MarketData.Yahoo.Dto;

// Shapes below are verified against live query1.finance.yahoo.com/v8/finance/chart responses
// (September 2026), including the error envelope, which Yahoo returns on both success and
// non-2xx responses (e.g. 404 "No data found, symbol may be delisted", 422 with a description
// naming the exact lookback limit that was exceeded).

internal sealed class YahooChartResponse
{
    [JsonPropertyName("chart")]
    public YahooChartEnvelope? Chart { get; set; }
}

internal sealed class YahooChartEnvelope
{
    [JsonPropertyName("result")]
    public List<YahooChartResult>? Result { get; set; }

    [JsonPropertyName("error")]
    public YahooApiError? Error { get; set; }
}

internal sealed class YahooApiError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal sealed class YahooChartResult
{
    [JsonPropertyName("meta")]
    public YahooChartMeta? Meta { get; set; }

    [JsonPropertyName("timestamp")]
    public List<long>? Timestamp { get; set; }

    [JsonPropertyName("indicators")]
    public YahooIndicators? Indicators { get; set; }
}

internal sealed class YahooChartMeta
{
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchangeName")]
    public string? ExchangeName { get; set; }

    [JsonPropertyName("instrumentType")]
    public string? InstrumentType { get; set; }

    [JsonPropertyName("regularMarketTime")]
    public long RegularMarketTime { get; set; }

    [JsonPropertyName("regularMarketPrice")]
    public decimal RegularMarketPrice { get; set; }

    // Only "chartPreviousClose" was present in verified live responses; "previousClose" shows
    // up for some query variants per community reports. Both kept, both optional.
    [JsonPropertyName("previousClose")]
    public decimal? PreviousClose { get; set; }

    [JsonPropertyName("chartPreviousClose")]
    public decimal? ChartPreviousClose { get; set; }

    [JsonPropertyName("regularMarketVolume")]
    public long? RegularMarketVolume { get; set; }

    [JsonPropertyName("longName")]
    public string? LongName { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("dataGranularity")]
    public string? DataGranularity { get; set; }

    [JsonPropertyName("range")]
    public string? Range { get; set; }

    [JsonPropertyName("validRanges")]
    public List<string>? ValidRanges { get; set; }
}

internal sealed class YahooIndicators
{
    [JsonPropertyName("quote")]
    public List<YahooQuoteIndicator>? Quote { get; set; }
}

internal sealed class YahooQuoteIndicator
{
    // Elements are null for timestamps Yahoo includes but has no trade data for - callers must
    // skip null rows rather than treat them as zero.
    [JsonPropertyName("open")]
    public List<decimal?>? Open { get; set; }

    [JsonPropertyName("high")]
    public List<decimal?>? High { get; set; }

    [JsonPropertyName("low")]
    public List<decimal?>? Low { get; set; }

    [JsonPropertyName("close")]
    public List<decimal?>? Close { get; set; }

    [JsonPropertyName("volume")]
    public List<long?>? Volume { get; set; }
}
