using Microsoft.Extensions.Caching.Memory;
using TradingViewReplica.MarketData;
using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Caching;

/// <summary>
/// Memory-only (no SQLite) - unlike candles, screener rankings are inherently ephemeral and not
/// worth persisting across restarts. Short TTL since rankings shift through the trading day.
/// </summary>
public sealed class ScreenerCacheStore : IScreenerCacheStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(1);

    private readonly IMarketDataProvider _provider;
    private readonly IMemoryCache _memoryCache;

    public ScreenerCacheStore(IMarketDataProvider provider, IMemoryCache memoryCache)
    {
        _provider = provider;
        _memoryCache = memoryCache;
    }

    public async Task<ScreenerResult> GetScreenerAsync(string screenerId, int count, CancellationToken cancellationToken)
    {
        var cacheKey = $"screener:{screenerId}:{count}";

        if (_memoryCache.TryGetValue(cacheKey, out ScreenerResult? cached) && cached is not null)
        {
            return cached;
        }

        var result = await _provider.GetScreenerAsync(screenerId, count, cancellationToken);
        _memoryCache.Set(cacheKey, result, Ttl);
        return result;
    }
}
