namespace TradingViewReplica.MarketData;

/// <summary>
/// Yahoo's screener endpoint is undocumented, but these predefined screener IDs were verified
/// live (September 2026) against query1.finance.yahoo.com/v1/finance/screener/predefined/saved.
/// A curated subset, not exhaustive - Yahoo has more (see finance.yahoo.com/research-hub/screener
/// for others) but these cover the common cases without chasing every one down individually.
/// ETF and crypto rows come back in the exact same shape as equities (quoteType differs -
/// "ETF"/"CRYPTOCURRENCY" vs "EQUITY" - but price/change/volume/marketCap all populate the same
/// way), so no backend changes were needed to support them beyond adding the screener IDs here.
/// </summary>
public static class PredefinedScreeners
{
    public static readonly IReadOnlyList<ScreenerDefinition> All =
    [
        new("most_actives", "Most Active", "Stocks"),
        new("day_gainers", "Day Gainers", "Stocks"),
        new("day_losers", "Day Losers", "Stocks"),
        new("growth_technology_stocks", "Growth Technology", "Stocks"),
        new("undervalued_large_caps", "Undervalued Large Caps", "Stocks"),
        new("undervalued_growth_stocks", "Undervalued Growth", "Stocks"),
        new("most_shorted_stocks", "Most Shorted", "Stocks"),
        new("aggressive_small_caps", "Small Cap Gainers", "Stocks"),

        new("top_etfs_us", "Top ETFs", "ETFs"),

        new("all_cryptocurrencies_us", "All Cryptocurrencies", "Crypto"),

        new("top_mutual_funds", "Top Mutual Funds", "Funds"),
        new("solid_large_growth_funds", "Large Growth Funds", "Funds"),
        new("solid_midcap_growth_funds", "Mid-Cap Growth Funds", "Funds"),
        new("conservative_foreign_funds", "Conservative Foreign Funds", "Funds"),
        new("portfolio_anchors", "Portfolio Anchors", "Funds"),
        new("high_yield_bond", "High Yield Bond", "Funds"),
    ];

    public static bool IsKnown(string screenerId) => All.Any(d => d.Id == screenerId);
}

public sealed record ScreenerDefinition(string Id, string Label, string Category);
