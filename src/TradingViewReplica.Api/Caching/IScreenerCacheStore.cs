using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Caching;

public interface IScreenerCacheStore
{
    Task<ScreenerResult> GetScreenerAsync(string screenerId, int count, CancellationToken cancellationToken);
}
