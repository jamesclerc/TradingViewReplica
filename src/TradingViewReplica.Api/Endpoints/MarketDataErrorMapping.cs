using TradingViewReplica.MarketData;

namespace TradingViewReplica.Api.Endpoints;

internal static class MarketDataErrorMapping
{
    public static IResult ToProblemResult(this MarketDataProviderException ex) => ex.ErrorType switch
    {
        MarketDataErrorType.SymbolNotFound => Results.NotFound(new { error = ex.Message }),
        MarketDataErrorType.RateLimited => Results.Json(
            new { error = ex.Message }, statusCode: StatusCodes.Status429TooManyRequests),
        MarketDataErrorType.SessionExpired => Results.Json(
            new { error = ex.Message }, statusCode: StatusCodes.Status503ServiceUnavailable),
        _ => Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway)
    };
}
