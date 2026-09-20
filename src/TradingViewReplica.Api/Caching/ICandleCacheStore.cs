using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Caching;

public interface ICandleCacheStore
{
    Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string symbol, Interval interval, MarketDataRange range, CancellationToken cancellationToken);
}
