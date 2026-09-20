using TradingViewReplica.Api.Caching;
using TradingViewReplica.Api.Contracts;
using TradingViewReplica.MarketData;

namespace TradingViewReplica.Api.Endpoints;

public static class ScreenerEndpoints
{
    public static IEndpointRouteBuilder MapScreenerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/screener/definitions", () =>
            Results.Ok(PredefinedScreeners.All.Select(d => new ScreenerDefinitionDto(d.Id, d.Label, d.Category))))
            .WithName("GetScreenerDefinitions");

        app.MapGet("/api/screener/{screenerId}", async (
            string screenerId,
            int? count,
            IScreenerCacheStore cacheStore,
            CancellationToken cancellationToken) =>
        {
            if (!PredefinedScreeners.IsKnown(screenerId))
            {
                return Results.BadRequest(new { error = $"Unknown screener '{screenerId}'." });
            }

            var resolvedCount = Math.Clamp(count ?? 25, 1, 100);

            try
            {
                var result = await cacheStore.GetScreenerAsync(screenerId, resolvedCount, cancellationToken);
                var dto = new ScreenerResultDto(
                    result.Title,
                    result.Total,
                    result.Quotes.Select(q => new ScreenerQuoteDto(
                        q.Symbol, q.DisplayName, q.Price, q.ChangePercent, q.Volume, q.MarketCap,
                        q.TrailingPE, q.DividendYield, q.AnalystRating, q.Exchange,
                        q.FiftyTwoWeekHigh, q.FiftyTwoWeekLow, q.DayHigh, q.DayLow,
                        q.AverageVolume3Month, q.ForwardPE, q.Eps))
                        .ToList());
                return Results.Ok(dto);
            }
            catch (MarketDataProviderException ex)
            {
                return ex.ToProblemResult();
            }
        })
        .WithName("GetScreener");

        return app;
    }
}
