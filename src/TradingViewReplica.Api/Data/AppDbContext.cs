using Microsoft.EntityFrameworkCore;
using TradingViewReplica.Api.Caching;
using TradingViewReplica.Api.Watchlist;

namespace TradingViewReplica.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();

    public DbSet<CachedCandle> CachedCandles => Set<CachedCandle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WatchlistItem>()
            .HasIndex(w => w.Symbol)
            .IsUnique();

        modelBuilder.Entity<CachedCandle>()
            .HasIndex(c => new { c.Symbol, c.Interval, c.TimestampUtc })
            .IsUnique();
    }
}
