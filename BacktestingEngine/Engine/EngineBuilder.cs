using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.Services;
using BacktestingEngine.Logging;
using BacktestingEngine.DTO;
using System.Diagnostics;

namespace BacktestingEngine.Engine
{
    public class EngineBuilder
    {
        private readonly IKlinesUploadFromDbService _klinesUploadFromDbService;
        private readonly ILogger<EngineBuilder> _logger;

        public EngineBuilder(IKlinesUploadFromDbService klinesUploadFromDbService, ILogger<EngineBuilder> logger)
        {
            _klinesUploadFromDbService = klinesUploadFromDbService;
            _logger = logger;
        }


        private BacktestStats Runner(IReadOnlyList<EngineCandle> candles, BacktestRunRequest request)
        {
            var portfolio = new Portfolio(request.StartingQuote);
            var exec = new ExecutionModel(request.FeeRate);
            var strat = new GridStrategy(request.GridConfig, candles[0].Open);
            var statistics = new StatisticsCollector(request);

            var cfg = strat.Build();
            var orderManager = new OrderManager(cfg);
            var sw = Stopwatch.StartNew();
            foreach (var candle in candles)
            {
                // Event-rules layer

                // Order layer
                // 1 Fill per candle for simplicity
                Fill? chosenFill = null;
                OrderLimit? order = null;
                PairOrder? pairOrder = null;
                MarketEvent? marketEvent = null;
                foreach (var pair in orderManager.PairOrders)
                {

                    if (exec.TryFill(pair, candle, out var fill) && portfolio.CanApplyFill(fill))
                    {
                        chosenFill = fill;
                        pairOrder = pair;
                        break;
                    }
                }

                if (chosenFill != null)
                {
                    portfolio.ApplyFill(chosenFill);
                    order = orderManager.CanApplyOrder(chosenFill);
                    marketEvent = new MarketEvent(pairOrder!, order!, chosenFill);
                }
                statistics.GetStatOnCandle(portfolio, candle, marketEvent);
            }
            sw.Stop();
            _logger.LogInformation("Backtest run completed in {ElapsedSeconds} seconds. Final equity: {FinalEquity}",
                sw.Elapsed.TotalSeconds, portfolio.Equity(candles[^1].Close));
            return statistics.Build();
        }

        public async Task<List<BacktestStats>> RunGridPairAsync(BacktestBatchRequest batchRequest)
        {
            var candles = await _klinesUploadFromDbService.GetKlinesForEngineAsync
            (
                batchRequest.RangeData
            );

            if (candles.Count == 0) throw new InvalidOperationException("No candles loaded.");

            var results = new List<BacktestStats>();

            foreach (var run in batchRequest.Runs)
            {
                var runResult = Runner(candles, run);
                _logger.LogInformation("Backtest #{Id} completed.",
                    runResult.Id);
                results.Add(runResult);
            }

            return results;
        }

    }

}