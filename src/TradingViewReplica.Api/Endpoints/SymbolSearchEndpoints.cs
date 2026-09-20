using TradingViewReplica.Api.Contracts;
using TradingViewReplica.MarketData;

namespace TradingViewReplica.Api.Endpoints;

public static class SymbolSearchEndpoints
{
    public static IEndpointRouteBuilder MapSymbolSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/symbols/search", async (
            string? q,
            IMarketDataProvider provider,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.Ok(Array.Empty<SymbolSearchResultDto>());
            }

            try
            {
                var results = await provider.SearchSymbolsAsync(q, cancellationToken);
                var dtos = results.Select(r => new SymbolSearchResultDto(
                    r.Symbol, r.DisplayName, r.LongName, r.ExchangeDisplay, r.QuoteType));
                return Results.Ok(dtos);
            }
            catch (MarketDataProviderException ex)
            {
                return ex.ToProblemResult();
            }
        })
        .WithName("SearchSymbols");

        return app;
    }
}
