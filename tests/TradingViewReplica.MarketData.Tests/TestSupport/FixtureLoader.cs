namespace TradingViewReplica.MarketData.Tests.TestSupport;

internal static class FixtureLoader
{
    public static string ReadText(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));
}
