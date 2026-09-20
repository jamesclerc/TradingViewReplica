namespace TradingViewReplica.MarketData.Models;

public sealed record ScreenerResult(string Title, int Total, IReadOnlyList<ScreenerQuote> Quotes);
