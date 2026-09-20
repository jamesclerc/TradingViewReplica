namespace TradingViewReplica.MarketData;

public enum MarketDataErrorType
{
    RateLimited,
    SessionExpired,
    SymbolNotFound,
    UpstreamError
}

public sealed class MarketDataProviderException : Exception
{
    public MarketDataErrorType ErrorType { get; }

    public MarketDataProviderException(MarketDataErrorType errorType, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }
}
