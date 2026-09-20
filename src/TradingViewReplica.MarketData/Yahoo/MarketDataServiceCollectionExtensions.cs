using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TradingViewReplica.MarketData.Yahoo;

public static class MarketDataServiceCollectionExtensions
{
    public static IServiceCollection AddYahooMarketData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YahooClientOptions>(configuration.GetSection(YahooClientOptions.SectionName));

        // UseCookies=false on both handlers: cookies are attached manually as an explicit
        // "Cookie" header (see YahooSessionManager), since mixing that with the handler's own
        // automatic cookie container throws at runtime.
        services.AddHttpClient(YahooClientOptions.FinanceClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<YahooClientOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = options.RequestTimeout;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

        services.AddHttpClient(YahooClientOptions.SessionClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<YahooClientOptions>>().Value;
                client.Timeout = options.RequestTimeout;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

        services.AddSingleton<RateLimiter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<YahooClientOptions>>().Value;
            return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = options.RateLimitPermits,
                TokensPerPeriod = options.RateLimitPermits,
                ReplenishmentPeriod = options.RateLimitWindow,
                QueueLimit = 50,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
        });

        services.AddSingleton<YahooSessionManager>();
        services.AddSingleton<YahooFinanceClient>();
        services.AddSingleton<IMarketDataProvider, YahooFinanceMarketDataProvider>();

        return services;
    }
}
