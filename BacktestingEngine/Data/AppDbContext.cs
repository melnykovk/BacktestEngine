using BacktestingEngine.Entity;
using Microsoft.EntityFrameworkCore;

namespace BacktestingEngine.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<KlineEntity> BinanceKlines => Set<KlineEntity>();
        public DbSet<KlineDatasetEntity> KlineDatasets => Set<KlineDatasetEntity>();
        public DbSet<BacktestBatchResultEntity> BatchResults => Set<BacktestBatchResultEntity>();


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


            var d = modelBuilder.Entity<KlineDatasetEntity>();
            d.ToTable("binance_klines_datasets", "public");
            d.Property(x => x.Symbol).HasColumnName("symbol");
            d.Property(x => x.Interval).HasColumnName("interval");
            d.Property(x => x.FirstOpenTimeMs).HasColumnName("first_open_time_ms");
            d.Property(x => x.LastOpenTimeMs).HasColumnName("last_open_time_ms");
            d.Property(x => x.CandlesCount).HasColumnName("candles_count");
            d.HasKey(x => new { x.Symbol, x.Interval });

            var r = modelBuilder.Entity<BacktestBatchResultEntity>();
            r.ToTable("backtest_batches", "public");
            r.HasKey(x => x.Id);
            r.Property(x => x.Name).HasMaxLength(200);
            r.Property(x => x.RangeData).HasColumnType("jsonb");
            r.Property(x => x.Stats).HasColumnType("jsonb");
        }
    }
}