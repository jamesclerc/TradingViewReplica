using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Caching;

public interface ILatestCandleCache
{
    Task<Candle?> GetLatestAsync(string symbol, Interval interval, CancellationToken cancellationToken);
}
