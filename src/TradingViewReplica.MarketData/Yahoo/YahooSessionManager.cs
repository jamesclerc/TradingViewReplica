namespace TradingViewReplica.MarketData.Yahoo;

/// <summary>
/// Owns Yahoo's cookie+crumb session lifecycle. Registered as a singleton: its whole purpose is
/// caching session state across requests, and refreshes are single-flighted so that several
/// concurrent 401s trigger exactly one real refresh instead of a stampede on Yahoo's endpoints.
/// </summary>
internal sealed class YahooSessionManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _cookieHeader;
    private string? _crumb;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public YahooSessionManager(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public bool HasValidSession => _crumb is not null && DateTimeOffset.UtcNow < _expiresAt;

    public string? CookieHeader => _cookieHeader;

    public string? Crumb => _crumb;

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (HasValidSession)
            {
                return;
            }

            var client = _httpClientFactory.CreateClient(YahooClientOptions.SessionClientName);

            using var warmupRequest = new HttpRequestMessage(HttpMethod.Get, "https://fc.yahoo.com");
            using var warmupResponse = await client.SendAsync(warmupRequest, cancellationToken);

            var cookies = ExtractCookies(warmupResponse);
            if (cookies is null)
            {
                throw SessionFailure("no session cookies were returned from the warm-up request");
            }

            using var crumbRequest = new HttpRequestMessage(
                HttpMethod.Get, "https://query2.finance.yahoo.com/v1/test/getcrumb");
            crumbRequest.Headers.Add("Cookie", cookies);
            using var crumbResponse = await client.SendAsync(crumbRequest, cancellationToken);

            if (!crumbResponse.IsSuccessStatusCode)
            {
                throw SessionFailure($"the crumb request failed with status {(int)crumbResponse.StatusCode}");
            }

            var crumb = (await crumbResponse.Content.ReadAsStringAsync(cancellationToken)).Trim();
            if (string.IsNullOrEmpty(crumb) || crumb.Contains('<'))
            {
                // A stray '<' means Yahoo returned an HTML page - almost always the EU consent
                // wall - instead of a crumb. That flow needs scraping a CSRF token out of a
                // consent form and isn't implemented here; fail loudly instead of guessing.
                throw SessionFailure("Yahoo returned an HTML page instead of a crumb, likely the EU consent wall");
            }

            _cookieHeader = cookies;
            _crumb = crumb;
            _expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static string? ExtractCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return null;
        }

        var pairs = setCookieHeaders
            .Select(header => header.Split(';', 2)[0])
            .Where(pair => pair.Contains('='))
            .ToList();

        return pairs.Count > 0 ? string.Join("; ", pairs) : null;
    }

    private static MarketDataProviderException SessionFailure(string detail) =>
        new(MarketDataErrorType.SessionExpired, $"Failed to establish a Yahoo Finance session: {detail}.");
}
