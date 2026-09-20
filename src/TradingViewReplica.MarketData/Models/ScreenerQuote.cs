namespace TradingViewReplica.MarketData.Models;

public sealed record ScreenerQuote(
    string Symbol,
    string DisplayName,
    decimal Price,
    decimal ChangePercent,
    long? Volume,
    long? MarketCap,
    decimal? TrailingPE,
    decimal? DividendYield,
    string? AnalystRating,
    string? Exchange,
    decimal? FiftyTwoWeekHigh,
    decimal? FiftyTwoWeekLow,
    decimal? DayHigh,
    decimal? DayLow,
    long? AverageVolume3Month,
    decimal? ForwardPE,
    decimal? Eps);
