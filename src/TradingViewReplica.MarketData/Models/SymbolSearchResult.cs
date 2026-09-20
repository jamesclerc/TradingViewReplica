namespace TradingViewReplica.MarketData.Models;

public sealed record SymbolSearchResult(
    string Symbol,
    string DisplayName,
    string? LongName,
    string? ExchangeDisplay,
    string? QuoteType);
