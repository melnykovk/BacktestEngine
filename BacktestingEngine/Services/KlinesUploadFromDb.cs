using BacktestingEngine.DTO;
using BacktestingEngine.Helpers;
using BacktestingEngine.Engine;
using BacktestingEngine.Entity;

namespace BacktestingEngine.Services
{
    public interface IKlinesUploadFromDbService
    {
        Task<IReadOnlyList<KlineDTO>> GetKlinesAsync(RangeBacktestData rangeData);

        Task<IReadOnlyList<EngineCandle>> GetKlinesForEngineAsync(RangeBacktestData rangeData);

        Task CheckKlinesOnErrors(RangeBacktestData rangeData);
        Task<IReadOnlyList<DbCandleSummaryDto>> GetCandleDatasetsSummaryAsync();
    }


    public class KlinesUploadFromDbService : IKlinesUploadFromDbService
    {
        private readonly ICandleRepository _candleRepository;
        private readonly ILogger<KlinesUploadFromDbService> _logger;
        public KlinesUploadFromDbService(ICandleRepository candleRepository, ILogger<KlinesUploadFromDbService> logger)
        {
            _candleRepository = candleRepository;
            _logger = logger;
        }


        public async Task<IReadOnlyList<KlineDTO>> GetKlinesAsync(RangeBacktestData rangeData)
        {

            var candles = await GetKlinesFromDBAsyncValidate(rangeData.Symbol, rangeData.Interval, rangeData.StartDate, rangeData.EndDate);

            return candles
            .Select(c => new KlineDTO(
                c.OpenTimeMs,
                c.Open,
                c.High,
                c.Low,
                c.Close,
                c.Volume,
                c.CloseTimeMs,
                c.QuoteAssetVol,
                c.TradesCount,
                c.TakerBuyBase,
                c.TakerBuyQuote))
            .ToList();
        }


        public async Task<IReadOnlyList<EngineCandle>> GetKlinesForEngineAsync(RangeBacktestData rangeData)
        {

            var candles = await GetKlinesFromDBAsyncValidate(rangeData.Symbol, rangeData.Interval, rangeData.StartDate, rangeData.EndDate);

            return candles
            .Select(c => new EngineCandle(
                c.OpenTimeMs,
                c.Open,
                c.High,
                c.Low,
                c.Close,
                c.Volume,
                c.CloseTimeMs
                ))
            .ToList();
        }


        private async Task<IReadOnlyList<KlineEntity>> GetKlinesFromDBAsyncValidate(string symbol, string interval, DateTime startDate, DateTime endDate)
        {

            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
            symbol = symbol.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(interval))
                throw new ArgumentException("Interval cannot be null or empty.", nameof(interval));
            interval = interval.Trim().ToLowerInvariant();


            var startTimeMs = TimeRange.StartOfDayMsUtc(startDate);
            var endTimeMs = TimeRange.EndOfDayExclusiveMsUtc(endDate);


            if (startTimeMs >= endTimeMs)
                throw new ArgumentException("Start date must be earlier than end date.", nameof(startDate));


            var candles = await _candleRepository.GetKlinesAsync(symbol, interval, startTimeMs, endTimeMs);

            return candles;
        }


        public async Task CheckKlinesOnErrors(RangeBacktestData rangeData)
        {

            var _marketContext = new MarketContext(rangeData.Symbol, rangeData.Interval);
            var klines = await GetKlinesForEngineAsync(rangeData);


            foreach (var k in klines)
            {
                var status = _marketContext.Advance(k);

                switch (status)
                {
                    case Enums.CandleStatus.Duplicate:
                        _logger.LogWarning("Duplicate candle. Symbol={Symbol}, Interval={Interval}, OpenTime={OpenTime}, Index={Index}",
                            rangeData.Symbol, rangeData.Interval, k.OpenTime, _marketContext.Index);
                        throw new InvalidOperationException("Duplicate candle encountered. Stopping processing.");

                    case Enums.CandleStatus.OutOfOrder:
                        _logger.LogError("Out of order. Symbol={Symbol}, Interval={Interval}, OpenTime={OpenTime}, Index={Index}",
                            rangeData.Symbol, rangeData.Interval, k.OpenTime, _marketContext.Index);
                        throw new InvalidOperationException("Out of order candle encountered. Stopping processing.");
                }
            }
            _logger.LogInformation("Successful. Finished processing klines for engine. Symbol={Symbol}, Interval={Interval}, TotalCandles={Index}",
                rangeData.Symbol, rangeData.Interval, _marketContext.Index);
        }


        public async Task<IReadOnlyList<DbCandleSummaryDto>> GetCandleDatasetsSummaryAsync()
        {
            return await _candleRepository.GetCandleDatasetsSummaryAsync();
        }

    }
}