namespace TradingViewReplica.Api.Contracts;

public sealed record ScreenerQuoteDto(
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

public sealed record ScreenerResultDto(string Title, int Total, IReadOnlyList<ScreenerQuoteDto> Quotes);

public sealed record ScreenerDefinitionDto(string Id, string Label, string Category);
