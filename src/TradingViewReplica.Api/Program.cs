using Microsoft.EntityFrameworkCore;
using TradingViewReplica.Api.Caching;
using TradingViewReplica.Api.Data;
using TradingViewReplica.Api.Endpoints;
using TradingViewReplica.MarketData.Yahoo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddYahooMarketData(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICandleCacheStore, CandleCacheStore>();
builder.Services.AddSingleton<IScreenerCacheStore, ScreenerCacheStore>();
builder.Services.AddSingleton<ILatestCandleCache, LatestCandleCache>();

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "tradingview-replica.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCandlesEndpoints();
app.MapSymbolSearchEndpoints();
app.MapScreenerEndpoints();

app.Run();
