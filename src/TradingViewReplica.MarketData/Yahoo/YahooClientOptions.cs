namespace TradingViewReplica.MarketData.Yahoo;

public sealed class YahooClientOptions
{
    public const string SectionName = "Yahoo";
    internal const string FinanceClientName = "YahooFinance";
    internal const string SessionClientName = "YahooSession";

    public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com";

    // A plain browser UA is sufficient for the chart/search endpoints as verified live - no
    // crumb/cookie required until Yahoo actually returns a 401.
    public string UserAgent { get; set; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0 Safari/537.36";

    public int RateLimitPermits { get; set; } = 5;

    public TimeSpan RateLimitWindow { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
