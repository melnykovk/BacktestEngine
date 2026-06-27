using GrindBotAPI.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrindBotAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<KlineEntity> BinanceKlines => Set<KlineEntity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<KlineEntity>();

            e.ToTable("binance_klines", "public");

            e.Property(x => x.Symbol).HasColumnName("symbol");
            e.Property(x => x.Interval).HasColumnName("interval");
            e.Property(x => x.OpenTimeMs).HasColumnName("open_time_ms");
            e.Property(x => x.Open).HasColumnName("open");
            e.Property(x => x.High).HasColumnName("high");
            e.Property(x => x.Low).HasColumnName("low");
            e.Property(x => x.Close).HasColumnName("close");
            e.Property(x => x.Volume).HasColumnName("volume");
            e.Property(x => x.CloseTimeMs).HasColumnName("close_time_ms");
            e.Property(x => x.QuoteAssetVol).HasColumnName("quote_asset_vol");
            e.Property(x => x.TradesCount).HasColumnName("trades_count");
            e.Property(x => x.TakerBuyBase).HasColumnName("taker_buy_base");
            e.Property(x => x.TakerBuyQuote).HasColumnName("taker_buy_quote");

            e.HasKey(x => new { x.Symbol, x.Interval, x.OpenTimeMs });

            e.Property(x => x.Open).HasPrecision(18, 8);
            e.Property(x => x.High).HasPrecision(18, 8);
            e.Property(x => x.Low).HasPrecision(18, 8);
            e.Property(x => x.Close).HasPrecision(18, 8);

            e.Property(x => x.Volume).HasPrecision(28, 8);
            e.Property(x => x.QuoteAssetVol).HasPrecision(28, 8);
            e.Property(x => x.TakerBuyBase).HasPrecision(28, 8);
            e.Property(x => x.TakerBuyQuote).HasPrecision(28, 8);
        }
    }
}