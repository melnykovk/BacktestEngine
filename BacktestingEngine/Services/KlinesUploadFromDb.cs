using BacktestingEngine.DTO;
using BacktestingEngine.Helpers;
using BacktestingEngine.Engine;
using BacktestingEngine.Entity;

namespace BacktestingEngine.Services
{
    public interface IKlinesUploadFromDbService
    {
        Task<IReadOnlyList<KlineDTO>> GetKlinesAsync(RangeBacktestData rangeData, CancellationToken _token);

        Task<EngineCandle[]> GetKlinesForEngineAsync(RangeBacktestData rangeData, CancellationToken _token);

        Task CheckKlinesOnErrors(RangeBacktestData rangeData, CancellationToken _token);
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

        
        public async Task<IReadOnlyList<KlineDTO>> GetKlinesAsync(RangeBacktestData rangeData, CancellationToken _token)
        {

            var candles = await GetKlinesFromDBAsyncValidate(rangeData.Symbol, rangeData.Interval, rangeData.StartDate, rangeData.EndDate, _token);

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


        public async Task<EngineCandle[]> GetKlinesForEngineAsync(RangeBacktestData rangeData, CancellationToken _token)
        {

            var candles = await GetKlinesFromDBAsyncValidate(rangeData.Symbol, rangeData.Interval, rangeData.StartDate, rangeData.EndDate, _token);
            var result = new EngineCandle[candles.Count];
            for (int i = 0; i < candles.Count; i++)
            {
                var candle = candles[i];
                _token.ThrowIfCancellationRequested();
                result[i] = new EngineCandle(
                    candle.OpenTimeMs,
                    candle.Open,
                    candle.High,
                    candle.Low,
                    candle.Close,
                    candle.Volume,
                    candle.CloseTimeMs
                );
            }
            return result;
        }


        private async Task<IReadOnlyList<KlineEntity>> GetKlinesFromDBAsyncValidate(string symbol, string interval, DateTime startDate, DateTime endDate, CancellationToken _token)
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


            var candles = await _candleRepository.GetKlinesAsync(symbol, interval, startTimeMs, endTimeMs, _token);

            return candles;
        }


        public async Task CheckKlinesOnErrors(RangeBacktestData rangeData, CancellationToken _token)
        {

            var _marketContext = new MarketContext(rangeData.Symbol, rangeData.Interval);
            var klines = await GetKlinesForEngineAsync(rangeData, _token);


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


    }
}