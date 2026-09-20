namespace TradingViewReplica.Api.Contracts;

public sealed record SymbolSearchResultDto(
    string Symbol, string DisplayName, string? LongName, string? ExchangeDisplay, string? QuoteType);
