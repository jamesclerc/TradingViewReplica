using TradingViewReplica.Api.Caching;
using TradingViewReplica.Api.Contracts;
using TradingViewReplica.MarketData;
using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.Api.Endpoints;

public static class CandlesEndpoints
{
    public static IEndpointRouteBuilder MapCandlesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/candles/{symbol}", async (
            string symbol,
            string? interval,
            string? range,
            ICandleCacheStore cacheStore,
            CancellationToken cancellationToken) =>
        {
            if (!TryParseInterval(interval, out var parsedInterval))
            {
                return Results.BadRequest(new { error = $"Unknown interval '{interval}'." });
            }

            if (!TryParseRange(range, out var parsedRange))
            {
                return Results.BadRequest(new { error = $"Unknown range '{range}'." });
            }

            try
            {
                var candles = await cacheStore.GetCandlesAsync(symbol, parsedInterval, parsedRange, cancellationToken);
                var dtos = candles.Select(c => new CandleDto(
                    c.Timestamp.ToUnixTimeMilliseconds(), c.Open, c.High, c.Low, c.Close, c.Volume));
                return Results.Ok(dtos);
            }
            catch (MarketDataProviderException ex)
            {
                return ex.ToProblemResult();
            }
        })
        .WithName("GetCandles");

        app.MapGet("/api/candles/{symbol}/latest", async (
            string symbol,
            string? interval,
            ILatestCandleCache latestCandleCache,
            CancellationToken cancellationToken) =>
        {
            if (!TryParseInterval(interval, out var parsedInterval))
            {
                return Results.BadRequest(new { error = $"Unknown interval '{interval}'." });
            }

            try
            {
                var candle = await latestCandleCache.GetLatestAsync(symbol, parsedInterval, cancellationToken);
                if (candle is null)
                {
                    return Results.NotFound(new { error = $"No recent bar available for '{symbol}'." });
                }

                var dto = new CandleDto(
                    candle.Timestamp.ToUnixTimeMilliseconds(), candle.Open, candle.High, candle.Low, candle.Close,
                    candle.Volume);
                return Results.Ok(dto);
            }
            catch (MarketDataProviderException ex)
            {
                return ex.ToProblemResult();
            }
        })
        .WithName("GetLatestCandle");

        return app;
    }

    private static bool TryParseInterval(string? value, out Interval interval)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            interval = Interval.OneDay;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out interval);
    }

    private static bool TryParseRange(string? value, out MarketDataRange range)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            range = MarketDataRange.SixMonths;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out range);
    }
}
