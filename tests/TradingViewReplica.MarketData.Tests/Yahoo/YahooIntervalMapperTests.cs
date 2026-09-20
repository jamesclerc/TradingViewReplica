using TradingViewReplica.MarketData.Models;
using TradingViewReplica.MarketData.Yahoo;

namespace TradingViewReplica.MarketData.Tests.Yahoo;

public class YahooIntervalMapperTests
{
    [Theory]
    [InlineData(Interval.OneMinute, "1m")]
    [InlineData(Interval.OneHour, "1h")]
    [InlineData(Interval.OneDay, "1d")]
    [InlineData(Interval.OneWeek, "1wk")]
    [InlineData(Interval.ThreeMonths, "3mo")]
    public void ToQueryValue_Interval_MapsToYahooSyntax(Interval interval, string expected)
    {
        Assert.Equal(expected, YahooIntervalMapper.ToQueryValue(interval));
    }

    [Theory]
    [InlineData(MarketDataRange.OneDay, "1d")]
    [InlineData(MarketDataRange.YearToDate, "ytd")]
    [InlineData(MarketDataRange.Max, "max")]
    public void ToQueryValue_Range_MapsToYahooSyntax(MarketDataRange range, string expected)
    {
        Assert.Equal(expected, YahooIntervalMapper.ToQueryValue(range));
    }

    [Fact]
    public void ClampRange_OneMinuteWithLargeRange_ClampsToFiveDays()
    {
        // Confirmed live: Yahoo hard-rejects 1m data older than ~8 days with an HTTP 422.
        var clamped = YahooIntervalMapper.ClampRange(Interval.OneMinute, MarketDataRange.OneYear);

        Assert.Equal(MarketDataRange.FiveDays, clamped);
    }

    [Fact]
    public void ClampRange_OneMinuteWithinLimit_LeavesRangeUnchanged()
    {
        var clamped = YahooIntervalMapper.ClampRange(Interval.OneMinute, MarketDataRange.OneDay);

        Assert.Equal(MarketDataRange.OneDay, clamped);
    }

    [Fact]
    public void ClampRange_FiveMinuteWithLargeRange_ClampsToOneMonth()
    {
        // Confirmed live: Yahoo hard-rejects 5m data older than 60 days with an HTTP 422
        // ("The requested range must be within the last 60 days."). OneMonth (~30 days) is a
        // deliberately conservative clamp safely under that limit.
        var clamped = YahooIntervalMapper.ClampRange(Interval.FiveMinutes, MarketDataRange.SixMonths);

        Assert.Equal(MarketDataRange.OneMonth, clamped);
    }

    [Fact]
    public void ClampRange_HourlyWithMaxRange_ClampsToTwoYears()
    {
        var clamped = YahooIntervalMapper.ClampRange(Interval.OneHour, MarketDataRange.Max);

        Assert.Equal(MarketDataRange.TwoYears, clamped);
    }

    [Fact]
    public void ClampRange_DailyWithMaxRange_LeavesRangeUnchanged()
    {
        var clamped = YahooIntervalMapper.ClampRange(Interval.OneDay, MarketDataRange.Max);

        Assert.Equal(MarketDataRange.Max, clamped);
    }
}
