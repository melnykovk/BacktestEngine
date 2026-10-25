using BacktestingEngine.Data;
using BacktestingEngine.Entity;
using BacktestingEngine.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BacktestingEngine.DTO;

namespace BacktestingEngine.Services
{
    public interface ICandleRepository
    {
        Task<List<KlineDatasetEntity>> GetKlineDatasetAsync();
        Task<KlineDatasetEntity?> GetKlineDatasetAsync(DbDatasetDeleteRequestDto request, CancellationToken _token);

        Task<List<KlineEntity>> GetKlinesAsync(string symbol, string interval, long startMs, long endMs, CancellationToken _token);
    }


    public class CandleRepository : ICandleRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CandleRepository> _logger;
        

        public CandleRepository(AppDbContext dbContext, ILogger<CandleRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<KlineEntity>> GetKlinesAsync(string symbol, string interval, long startMs, long endMs, CancellationToken _token)
        {

            var candlesBefore = await _dbContext.BinanceKlines
                .AsNoTracking()
                .Where(x => x.Symbol == symbol)
                .Where(x => x.Interval == interval)
                .Where(x => x.OpenTimeMs <= startMs)
                .AnyAsync();

            var candlesAfter = await _dbContext.BinanceKlines
                .AsNoTracking()
                .Where(x => x.Symbol == symbol)
                .Where(x => x.Interval == interval)
                .Where(x => x.CloseTimeMs >= endMs)
                .AnyAsync();
            if (!candlesBefore && !candlesAfter)
            {
                throw new InvalidOperationException(
                    $"No candles found at the requested range " +
                    $"for {symbol} ({interval}).");
            }

            var candles = await _dbContext.BinanceKlines
                    .AsNoTracking()
                    .Where(k => k.Symbol == symbol)
                    .Where(k => k.Interval == interval)
                    .Where(k => k.OpenTimeMs >= startMs && k.OpenTimeMs < endMs)
                    .OrderBy(k => k.OpenTimeMs)
                    .ToListAsync(_token);
                    return candles;
        }

        public async Task<List<KlineDatasetEntity>> GetKlineDatasetAsync()
        {
            var summary = await _dbContext.KlineDatasets
            .AsNoTracking()
            .Select(k => new KlineDatasetEntity
            {
                Symbol = k.Symbol,
                Interval = k.Interval,
                FirstOpenTimeMs = k.FirstOpenTimeMs,
                LastOpenTimeMs = k.LastOpenTimeMs,
                CandlesCount = k.CandlesCount
            })
            .OrderBy(x => x.Symbol)
            .ThenBy(x => x.Interval)
            .ToListAsync();

            return summary;
        }

        public async Task<KlineDatasetEntity?> GetKlineDatasetAsync(DbDatasetDeleteRequestDto request, CancellationToken _token)
        {
            var summary = await _dbContext.KlineDatasets
            .AsNoTracking()
            .Where(k => k.Symbol == request.Symbol && k.Interval == request.Interval)
            .Select(k => new KlineDatasetEntity
            {
                Symbol = k.Symbol,
                Interval = k.Interval,
                FirstOpenTimeMs = k.FirstOpenTimeMs,
                LastOpenTimeMs = k.LastOpenTimeMs,
                CandlesCount = k.CandlesCount
            }).SingleOrDefaultAsync(_token);

            return summary;
        }

    }
}