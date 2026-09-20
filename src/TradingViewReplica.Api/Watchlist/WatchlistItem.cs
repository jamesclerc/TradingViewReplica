namespace TradingViewReplica.Api.Watchlist;

public sealed class WatchlistItem
{
    public int Id { get; set; }

    public required string Symbol { get; set; }

    public int DisplayOrder { get; set; }

    // DateTime, not DateTimeOffset - see the comment on CachedCandle.TimestampUtc.
    public DateTime AddedAtUtc { get; set; }
}
