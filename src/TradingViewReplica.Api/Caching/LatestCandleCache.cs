using Microsoft.Extensions.Caching.Memory;
using TradingViewReplica.MarketData;
using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Caching;

/// <summary>
/// Deliberately separate from <see cref="ICandleCacheStore"/>: that one caches a full requested
/// range for minutes at a time (right for loading a chart), this one exists to be polled every
/// few seconds for "what's the most recent bar" to fake a live-updating chart without building
/// out SignalR/a subscription registry/a background poller - just a short-TTL memory cache in
/// front of a small Yahoo request, polled directly by the client.
/// </summary>
public sealed class LatestCandleCache : ILatestCandleCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(5);

    private readonly IMarketDataProvider _provider;
    private readonly IMemoryCache _memoryCache;

    public LatestCandleCache(IMarketDataProvider provider, IMemoryCache memoryCache)
    {
        _provider = provider;
        _memoryCache = memoryCache;
    }

    public async Task<Candle?> GetLatestAsync(string symbol, Interval interval, CancellationToken cancellationToken)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var cacheKey = $"latest-candle:{normalizedSymbol}:{interval}";

        if (_memoryCache.TryGetValue(cacheKey, out Candle? cached))
        {
            return cached;
        }

        var range = LatestBarRangeFor(interval);
        var candles = await _provider.GetCandlesAsync(normalizedSymbol, interval, range, cancellationToken);
        var latest = candles.Count > 0 ? candles[^1] : null;

        _memoryCache.Set(cacheKey, latest, Ttl);
        return latest;
    }

    // Yahoo needs enough range to guarantee at least one bar comes back for the given interval
    // (e.g. a weekly interval returns nothing for a 1-day range) - not the range the chart itself
    // is displaying, which is a separate, usually much larger, concern.
    private static MarketDataRange LatestBarRangeFor(Interval interval) => interval switch
    {
        Interval.OneMinute or Interval.TwoMinutes or Interval.FiveMinutes or Interval.FifteenMinutes
            or Interval.ThirtyMinutes or Interval.SixtyMinutes or Interval.NinetyMinutes or Interval.OneHour
            => MarketDataRange.FiveDays,
        _ => MarketDataRange.OneMonth
    };
}
