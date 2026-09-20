using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TradingViewReplica.Api.Data;
using TradingViewReplica.MarketData;
using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Caching;

/// <summary>
/// Two-level read-through cache in front of <see cref="IMarketDataProvider"/>: IMemoryCache (L1,
/// keyed per exact symbol/interval/range request) backed by SQLite (L2, keyed per symbol/interval
/// - survives process restarts so a `dotnet watch` reload doesn't re-risk Yahoo's crumb/rate-limit
/// churn). Each (symbol, interval) pair is refreshed as a whole unit on a TTL rather than tracking
/// per-candle gaps - simpler and safe, at the cost of re-fetching the full range on a stale miss.
/// </summary>
public sealed class CandleCacheStore : ICandleCacheStore
{
    private readonly AppDbContext _dbContext;
    private readonly IMarketDataProvider _provider;
    private readonly IMemoryCache _memoryCache;

    public CandleCacheStore(AppDbContext dbContext, IMarketDataProvider provider, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _provider = provider;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string symbol, Interval interval, MarketDataRange range, CancellationToken cancellationToken)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var memoryCacheKey = $"candles:{normalizedSymbol}:{interval}:{range}";

        if (_memoryCache.TryGetValue(memoryCacheKey, out IReadOnlyList<Candle>? cached) && cached is not null)
        {
            return cached;
        }

        var ttl = CacheTtlFor(interval);
        var freshnessCutoff = DateTime.UtcNow - ttl;
        var intervalKey = interval.ToString();

        var newestFetch = await _dbContext.CachedCandles
            .Where(c => c.Symbol == normalizedSymbol && c.Interval == intervalKey)
            .OrderByDescending(c => c.FetchedAtUtc)
            .Select(c => (DateTime?)c.FetchedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        IReadOnlyList<Candle> candles;
        if (newestFetch is { } fetchedAt && fetchedAt >= freshnessCutoff)
        {
            var rows = await _dbContext.CachedCandles
                .Where(c => c.Symbol == normalizedSymbol && c.Interval == intervalKey)
                .OrderBy(c => c.TimestampUtc)
                .ToListAsync(cancellationToken);

            // Projected in memory, not in the query above: SQLite/EF can store DateTime fine but
            // constructing a DateTimeOffset from it needs an explicit UTC Kind, which doesn't
            // translate to SQL.
            candles = rows
                .Select(c => new Candle(
                    new DateTimeOffset(DateTime.SpecifyKind(c.TimestampUtc, DateTimeKind.Utc)),
                    c.Open, c.High, c.Low, c.Close, c.Volume))
                .ToList();
        }
        else
        {
            candles = await _provider.GetCandlesAsync(normalizedSymbol, interval, range, cancellationToken);
            await ReplaceCachedRowsAsync(normalizedSymbol, intervalKey, candles, cancellationToken);
        }

        _memoryCache.Set(memoryCacheKey, candles, ttl);
        return candles;
    }

    private async Task ReplaceCachedRowsAsync(
        string symbol, string intervalKey, IReadOnlyList<Candle> candles, CancellationToken cancellationToken)
    {
        await _dbContext.CachedCandles
            .Where(c => c.Symbol == symbol && c.Interval == intervalKey)
            .ExecuteDeleteAsync(cancellationToken);

        var fetchedAt = DateTime.UtcNow;
        _dbContext.CachedCandles.AddRange(candles.Select(c => new CachedCandle
        {
            Symbol = symbol,
            Interval = intervalKey,
            TimestampUtc = c.Timestamp.UtcDateTime,
            Open = c.Open,
            High = c.High,
            Low = c.Low,
            Close = c.Close,
            Volume = c.Volume,
            FetchedAtUtc = fetchedAt
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TimeSpan CacheTtlFor(Interval interval) => interval switch
    {
        Interval.OneDay or Interval.FiveDays or Interval.OneWeek or Interval.OneMonth or Interval.ThreeMonths
            => TimeSpan.FromMinutes(15),
        _ => TimeSpan.FromMinutes(1)
    };
}
