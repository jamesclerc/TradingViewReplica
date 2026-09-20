namespace TradingViewReplica.MarketData.Models;

public sealed record Candle(
    DateTimeOffset Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume);
