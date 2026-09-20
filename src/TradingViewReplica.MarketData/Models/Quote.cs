namespace TradingViewReplica.MarketData.Models;

public sealed record Quote(
    string Symbol,
    decimal RegularMarketPrice,
    decimal? PreviousClose,
    long? RegularMarketVolume,
    DateTimeOffset RegularMarketTime,
    string? Currency,
    string? ExchangeName,
    string? DisplayName);
