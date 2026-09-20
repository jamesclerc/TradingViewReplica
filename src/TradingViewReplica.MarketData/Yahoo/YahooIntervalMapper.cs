using TradingViewReplica.MarketData.Models;

namespace TradingViewReplica.MarketData.Yahoo;

internal static class YahooIntervalMapper
{
    public static string ToQueryValue(Interval interval) => interval switch
    {
        Interval.OneMinute => "1m",
        Interval.TwoMinutes => "2m",
        Interval.FiveMinutes => "5m",
        Interval.FifteenMinutes => "15m",
        Interval.ThirtyMinutes => "30m",
        Interval.SixtyMinutes => "60m",
        Interval.NinetyMinutes => "90m",
        Interval.OneHour => "1h",
        Interval.OneDay => "1d",
        Interval.FiveDays => "5d",
        Interval.OneWeek => "1wk",
        Interval.OneMonth => "1mo",
        Interval.ThreeMonths => "3mo",
        _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
    };

    public static string ToQueryValue(MarketDataRange range) => range switch
    {
        MarketDataRange.OneDay => "1d",
        MarketDataRange.FiveDays => "5d",
        MarketDataRange.OneMonth => "1mo",
        MarketDataRange.ThreeMonths => "3mo",
        MarketDataRange.SixMonths => "6mo",
        MarketDataRange.YearToDate => "ytd",
        MarketDataRange.OneYear => "1y",
        MarketDataRange.TwoYears => "2y",
        MarketDataRange.FiveYears => "5y",
        MarketDataRange.TenYears => "10y",
        MarketDataRange.Max => "max",
        _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
    };

    /// <summary>
    /// Yahoo hard-rejects (HTTP 422) intraday requests whose range exceeds its per-interval
    /// lookback window instead of truncating - confirmed live: requesting interval=1m&amp;range=1mo
    /// returns "Only 8 days worth of 1m granularity data are allowed to be fetched per request.",
    /// and interval=5m&amp;range=3mo returns "The requested range must be within the last 60 days."
    /// These limits are undocumented and can drift over time; the thresholds below were verified
    /// live in September 2026 (1m and 5m directly; 2m/15m/30m/90m conservatively assumed to share
    /// 5m's ~60-day window rather than tested individually - being more conservative than Yahoo's
    /// actual limit only means fetching less history than allowed, never a 422) and should be
    /// re-checked periodically, not trusted indefinitely.
    /// </summary>
    public static MarketDataRange ClampRange(Interval interval, MarketDataRange requestedRange)
    {
        var maxAllowed = MaxRangeFor(interval);
        return RangeRank(requestedRange) > RangeRank(maxAllowed) ? maxAllowed : requestedRange;
    }

    private static MarketDataRange MaxRangeFor(Interval interval) => interval switch
    {
        Interval.OneMinute => MarketDataRange.FiveDays,
        Interval.TwoMinutes or Interval.FiveMinutes or Interval.FifteenMinutes
            or Interval.ThirtyMinutes or Interval.NinetyMinutes => MarketDataRange.OneMonth,
        Interval.SixtyMinutes or Interval.OneHour => MarketDataRange.TwoYears,
        _ => MarketDataRange.Max
    };

    private static int RangeRank(MarketDataRange range) => range switch
    {
        MarketDataRange.OneDay => 0,
        MarketDataRange.FiveDays => 1,
        MarketDataRange.OneMonth => 2,
        MarketDataRange.ThreeMonths => 3,
        MarketDataRange.SixMonths => 4,
        MarketDataRange.YearToDate => 5,
        MarketDataRange.OneYear => 6,
        MarketDataRange.TwoYears => 7,
        MarketDataRange.FiveYears => 8,
        MarketDataRange.TenYears => 9,
        MarketDataRange.Max => 10,
        _ => 0
    };
}
