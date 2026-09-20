using TradingViewReplica.MarketData.Models;
using TradingViewReplica.MarketData.Yahoo.Dto;

namespace TradingViewReplica.MarketData.Yahoo;

internal sealed class YahooFinanceMarketDataProvider : IMarketDataProvider
{
    private readonly YahooFinanceClient _client;

    public YahooFinanceMarketDataProvider(YahooFinanceClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<SymbolSearchResult>> SearchSymbolsAsync(
        string query, CancellationToken cancellationToken = default)
    {
        var quotes = await _client.SearchAsync(query, cancellationToken);
        return quotes
            .Where(q => !string.IsNullOrEmpty(q.Symbol))
            .Select(q => new SymbolSearchResult(
                q.Symbol!,
                q.ShortName ?? q.Symbol!,
                q.LongName,
                q.ExchangeDisplay ?? q.Exchange,
                q.QuoteType))
            .ToList();
    }

    public async Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string symbol, Interval interval, MarketDataRange range, CancellationToken cancellationToken = default)
    {
        var clampedRange = YahooIntervalMapper.ClampRange(interval, range);
        var result = await _client.GetChartAsync(
            symbol,
            YahooIntervalMapper.ToQueryValue(interval),
            YahooIntervalMapper.ToQueryValue(clampedRange),
            cancellationToken);

        return MapCandles(result);
    }

    public async Task<Quote> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        // A small fixed request purely to get a fresh `meta` block - meta always carries the
        // live snapshot regardless of the requested range/interval, so there's no need to pull
        // a full history just to read the current price.
        var result = await _client.GetChartAsync(symbol, "1d", "1d", cancellationToken);
        return MapQuote(result);
    }

    public async Task<IReadOnlyList<Quote>> GetQuotesAsync(
        IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default)
    {
        var quotes = new List<Quote>(symbols.Count);
        foreach (var symbol in symbols)
        {
            quotes.Add(await GetQuoteAsync(symbol, cancellationToken));
        }

        return quotes;
    }

    public async Task<ScreenerResult> GetScreenerAsync(
        string screenerId, int count, CancellationToken cancellationToken = default)
    {
        var result = await _client.GetScreenerAsync(screenerId, count, cancellationToken);
        var quotes = (result.Quotes ?? []).Select(MapScreenerQuote).ToList();
        return new ScreenerResult(result.Title ?? screenerId, result.Total, quotes);
    }

    internal static ScreenerQuote MapScreenerQuote(YahooScreenerQuote q) => new(
        q.Symbol ?? string.Empty,
        q.DisplayName ?? q.ShortName ?? q.Symbol ?? string.Empty,
        q.RegularMarketPrice ?? 0,
        q.RegularMarketChangePercent ?? 0,
        q.RegularMarketVolume,
        q.MarketCap,
        q.TrailingPE,
        q.DividendYield,
        q.AverageAnalystRating,
        q.FullExchangeName ?? q.Exchange,
        q.FiftyTwoWeekHigh,
        q.FiftyTwoWeekLow,
        q.RegularMarketDayHigh,
        q.RegularMarketDayLow,
        q.AverageDailyVolume3Month,
        q.ForwardPE,
        q.EpsTrailingTwelveMonths);

    internal static IReadOnlyList<Candle> MapCandles(YahooChartResult result)
    {
        var timestamps = result.Timestamp;
        var quote = result.Indicators?.Quote?.FirstOrDefault();
        if (timestamps is null || quote is null)
        {
            return [];
        }

        var candles = new List<Candle>(timestamps.Count);
        for (var i = 0; i < timestamps.Count; i++)
        {
            var open = quote.Open?.ElementAtOrDefault(i);
            var high = quote.High?.ElementAtOrDefault(i);
            var low = quote.Low?.ElementAtOrDefault(i);
            var close = quote.Close?.ElementAtOrDefault(i);
            var volume = quote.Volume?.ElementAtOrDefault(i);

            // Yahoo leaves gaps (nulls) in these arrays for timestamps with no trade data -
            // skip rather than surfacing a fabricated zero-candle.
            if (open is null || high is null || low is null || close is null)
            {
                continue;
            }

            candles.Add(new Candle(
                DateTimeOffset.FromUnixTimeSeconds(timestamps[i]),
                open.Value,
                high.Value,
                low.Value,
                close.Value,
                volume ?? 0));
        }

        return candles;
    }

    internal static Quote MapQuote(YahooChartResult result)
    {
        var meta = result.Meta;
        if (meta?.Symbol is null)
        {
            throw new MarketDataProviderException(
                MarketDataErrorType.UpstreamError, "Yahoo chart response was missing quote metadata.");
        }

        return new Quote(
            meta.Symbol,
            meta.RegularMarketPrice,
            meta.ChartPreviousClose ?? meta.PreviousClose,
            meta.RegularMarketVolume,
            DateTimeOffset.FromUnixTimeSeconds(meta.RegularMarketTime),
            meta.Currency,
            meta.ExchangeName,
            meta.ShortName ?? meta.LongName);
    }
}
