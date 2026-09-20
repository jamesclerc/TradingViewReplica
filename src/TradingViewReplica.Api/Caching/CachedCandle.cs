namespace TradingViewReplica.Api.Caching;

public sealed class CachedCandle
{
    public int Id { get; set; }

    public required string Symbol { get; set; }

    /// <summary>String form of <see cref="TradingViewReplica.MarketData.Models.Interval"/>.</summary>
    public required string Interval { get; set; }

    // DateTime (not DateTimeOffset): EF Core's SQLite provider can't translate ORDER BY over
    // DateTimeOffset columns ("SQLite does not support expressions of type 'DateTimeOffset' in
    // ORDER BY clauses") - confirmed by a live 500 during Phase 1 smoke testing. Always UTC.
    public DateTime TimestampUtc { get; set; }

    public decimal Open { get; set; }

    public decimal High { get; set; }

    public decimal Low { get; set; }

    public decimal Close { get; set; }

    public long Volume { get; set; }

    public DateTime FetchedAtUtc { get; set; }
}
