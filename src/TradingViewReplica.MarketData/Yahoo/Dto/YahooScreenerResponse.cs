using System.Text.Json.Serialization;

namespace TradingViewReplica.MarketData.Yahoo.Dto;

// Verified against a live query1.finance.yahoo.com/v1/finance/screener/predefined/saved response
// (September 2026). The real payload also carries criteriaMeta/rawCriteria (the filter definition
// itself) - intentionally not modeled, we only need the result rows. Note: "sector"/"industry" are
// present in the field list Yahoo advertises but were consistently empty in every row observed
// live, so they're not modeled here - don't rely on them.

internal sealed class YahooScreenerResponse
{
    [JsonPropertyName("finance")]
    public YahooScreenerFinance? Finance { get; set; }
}

internal sealed class YahooScreenerFinance
{
    [JsonPropertyName("result")]
    public List<YahooScreenerResult>? Result { get; set; }

    [JsonPropertyName("error")]
    public YahooApiError? Error { get; set; }
}

internal sealed class YahooScreenerResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("quotes")]
    public List<YahooScreenerQuote>? Quotes { get; set; }
}

internal sealed class YahooScreenerQuote
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; set; }

    [JsonPropertyName("regularMarketChangePercent")]
    public decimal? RegularMarketChangePercent { get; set; }

    [JsonPropertyName("regularMarketVolume")]
    public long? RegularMarketVolume { get; set; }

    [JsonPropertyName("marketCap")]
    public long? MarketCap { get; set; }

    [JsonPropertyName("trailingPE")]
    public decimal? TrailingPE { get; set; }

    // Already percentage-scaled (e.g. 1.53 means 1.53%) - distinct from the separate
    // "trailingAnnualDividendYield" field Yahoo also returns, which is a decimal fraction.
    [JsonPropertyName("dividendYield")]
    public decimal? DividendYield { get; set; }

    [JsonPropertyName("averageAnalystRating")]
    public string? AverageAnalystRating { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("fullExchangeName")]
    public string? FullExchangeName { get; set; }

    [JsonPropertyName("fiftyTwoWeekHigh")]
    public decimal? FiftyTwoWeekHigh { get; set; }

    [JsonPropertyName("fiftyTwoWeekLow")]
    public decimal? FiftyTwoWeekLow { get; set; }

    [JsonPropertyName("regularMarketDayHigh")]
    public decimal? RegularMarketDayHigh { get; set; }

    [JsonPropertyName("regularMarketDayLow")]
    public decimal? RegularMarketDayLow { get; set; }

    [JsonPropertyName("averageDailyVolume3Month")]
    public long? AverageDailyVolume3Month { get; set; }

    [JsonPropertyName("forwardPE")]
    public decimal? ForwardPE { get; set; }

    [JsonPropertyName("epsTrailingTwelveMonths")]
    public decimal? EpsTrailingTwelveMonths { get; set; }
}
