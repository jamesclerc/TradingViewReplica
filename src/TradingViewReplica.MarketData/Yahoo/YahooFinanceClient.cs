using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using TradingViewReplica.MarketData.Yahoo.Dto;

namespace TradingViewReplica.MarketData.Yahoo;

/// <summary>
/// Talks to Yahoo Finance's unofficial endpoints. Requests go out anonymously first (verified
/// live to work for both /v8/finance/chart and /v1/finance/search); only on a 401 does it fall
/// back to establishing a cookie+crumb session via <see cref="YahooSessionManager"/> and retry
/// once. Registered as a singleton alongside a shared <see cref="RateLimiter"/> so the outbound
/// rate limit is global, not per-request.
/// </summary>
internal sealed class YahooFinanceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly YahooSessionManager _sessionManager;
    private readonly YahooClientOptions _options;
    private readonly RateLimiter _rateLimiter;

    public YahooFinanceClient(
        IHttpClientFactory httpClientFactory,
        YahooSessionManager sessionManager,
        IOptions<YahooClientOptions> options,
        RateLimiter rateLimiter)
    {
        _httpClientFactory = httpClientFactory;
        _sessionManager = sessionManager;
        _options = options.Value;
        _rateLimiter = rateLimiter;
    }

    public async Task<YahooChartResult> GetChartAsync(
        string symbol, string interval, string range, CancellationToken cancellationToken)
    {
        var path = $"/v8/finance/chart/{Uri.EscapeDataString(symbol)}?interval={interval}&range={range}";
        using var response = await SendAsync(path, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<YahooChartResponse>(JsonOptions, cancellationToken);
        var result = payload?.Chart?.Result?.FirstOrDefault();
        if (result is null)
        {
            throw new MarketDataProviderException(
                MarketDataErrorType.UpstreamError, $"Yahoo returned an empty chart result for '{symbol}'.");
        }

        return result;
    }

    public async Task<YahooScreenerResult> GetScreenerAsync(
        string screenerId, int count, CancellationToken cancellationToken)
    {
        var path = $"/v1/finance/screener/predefined/saved?scrIds={Uri.EscapeDataString(screenerId)}&count={count}";
        using var response = await SendAsync(path, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<YahooScreenerResponse>(JsonOptions, cancellationToken);
        var result = payload?.Finance?.Result?.FirstOrDefault();
        if (result is null)
        {
            throw new MarketDataProviderException(
                MarketDataErrorType.UpstreamError, $"Yahoo returned an empty screener result for '{screenerId}'.");
        }

        return result;
    }

    public async Task<IReadOnlyList<YahooSearchQuote>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var path = $"/v1/finance/search?q={Uri.EscapeDataString(query)}&quotesCount=10&newsCount=0";
        using var response = await SendAsync(path, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<YahooSearchResponse>(JsonOptions, cancellationToken);
        return payload?.Quotes ?? [];
    }

    private async Task<HttpResponseMessage> SendAsync(string relativePath, CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, cancellationToken);
        if (!lease.IsAcquired)
        {
            throw new MarketDataProviderException(
                MarketDataErrorType.RateLimited, "Local outbound rate limit to Yahoo Finance was exceeded.");
        }

        var response = await SendOnceAsync(relativePath, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            await _sessionManager.RefreshAsync(cancellationToken);
            response = await SendOnceAsync(relativePath, cancellationToken);
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        throw await BuildExceptionAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(string relativePath, CancellationToken cancellationToken)
    {
        var cookie = _sessionManager.CookieHeader;
        var crumb = _sessionManager.Crumb;

        var path = relativePath;
        if (crumb is not null)
        {
            var separator = path.Contains('?') ? '&' : '?';
            path = $"{path}{separator}crumb={Uri.EscapeDataString(crumb)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        var client = _httpClientFactory.CreateClient(YahooClientOptions.FinanceClientName);
        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task<MarketDataProviderException> BuildExceptionAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? description = null;
        try
        {
            var body = await response.Content.ReadFromJsonAsync<YahooChartResponse>(JsonOptions, cancellationToken);
            description = body?.Chart?.Error?.Description;
        }
        catch (JsonException)
        {
            // Non-JSON error body (e.g. an HTML error page) - fall back to the status code alone.
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new MarketDataProviderException(
                MarketDataErrorType.SymbolNotFound, description ?? "Symbol not found."),
            HttpStatusCode.TooManyRequests => new MarketDataProviderException(
                MarketDataErrorType.RateLimited, description ?? "Yahoo Finance rate-limited this request."),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new MarketDataProviderException(
                MarketDataErrorType.SessionExpired, description ?? "Yahoo Finance rejected the request session."),
            _ => new MarketDataProviderException(
                MarketDataErrorType.UpstreamError, description ?? $"Yahoo Finance returned HTTP {(int)response.StatusCode}.")
        };
    }
}
