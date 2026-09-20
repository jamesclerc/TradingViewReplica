namespace TradingViewReplica.Api.Contracts;

/// <summary>TimestampMs is milliseconds-since-epoch to match klinecharts' expected bar shape directly.</summary>
public sealed record CandleDto(long TimestampMs, decimal Open, decimal High, decimal Low, decimal Close, long Volume);
