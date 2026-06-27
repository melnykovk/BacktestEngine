using GrindBotAPI.Data;
using GrindBotAPI.Entity;
using GrindBotAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrindBotAPI.DTO;

namespace GrindBotAPI.Services
{
    public interface ICandleRepository
    {
        Task<List<KlineEntity>> GetKlinesAsync(
            string symbol,
            string interval,
            long startMs,
            long endMs);

        Task<IReadOnlyList<DbCandleSummaryDto>> GetCandleDatasetsSummaryAsync();
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

        public async Task<List<KlineEntity>> GetKlinesAsync(string symbol, string interval, long startMs, long endMs)
        {
            return await _dbContext.BinanceKlines
                    .AsNoTracking()
                    .Where(k => k.Symbol == symbol)
                    .Where(k => k.Interval == interval)
                    .Where(k => k.OpenTimeMs >= startMs && k.OpenTimeMs < endMs)
                    .OrderBy(k => k.OpenTimeMs)
                    .ToListAsync();
        }

        public async Task<IReadOnlyList<DbCandleSummaryDto>> GetCandleDatasetsSummaryAsync()
        {
            var summary = await _dbContext.BinanceKlines
            .AsNoTracking()
            .GroupBy(k => new { k.Symbol, k.Interval })
            .Select(g => new DbCandleSummaryDto
            {
                Symbol = g.Key.Symbol,
                Interval = g.Key.Interval,
                FirstOpenTimeMs = g.Min(k => k.OpenTimeMs),
                LastOpenTimeMs = g.Max(k => k.OpenTimeMs),
                CandlesCount = g.Count()
            })
            .OrderBy(x => x.Symbol)
            .ThenBy(x => x.Interval)
            .ToListAsync();

            return summary;
        }
    }
}