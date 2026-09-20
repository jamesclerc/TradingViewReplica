using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.MarketData;

/// <summary>
/// The only seam the rest of the app codes against. Yahoo Finance's unofficial endpoints are
/// the sole implementation today, but they can change or get rate-limited without notice - this
/// interface exists so a future provider swap (Alpha Vantage, Polygon, Finnhub, ...) doesn't
/// ripple through the API, caching, or SignalR layers.
/// </summary>
public interface IMarketDataProvider
{
    Task<IReadOnlyList<SymbolSearchResult>> SearchSymbolsAsync(string query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Candle>> GetCandlesAsync(string symbol, Interval interval, MarketDataRange range, CancellationToken cancellationToken = default);

    Task<Quote> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Quote>> GetQuotesAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default);

    Task<ScreenerResult> GetScreenerAsync(string screenerId, int count, CancellationToken cancellationToken = default);
}
