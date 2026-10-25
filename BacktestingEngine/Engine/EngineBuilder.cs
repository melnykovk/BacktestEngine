using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.Services;
using BacktestingEngine.Logging;
using BacktestingEngine.DTO;
using System.Diagnostics;
using BacktestingEngine.Helpers;

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

        private BacktestStats Runner(EngineCandle[] candles, BacktestRunRequest request, CancellationToken _token)
        {
            try
            {
                var portfolio = new Portfolio(request.StartingQuote);
                var exec = new ExecutionModel(request.FeeRate);
                var strat = new GridStrategy(request.GridConfig, candles[0].Open);
                var cfg = strat.Build();
                var orderManager = new OrderManager(cfg);
                var marketContext = new MarketContext(default!, default!);

                
                var statistics = new StatisticsCollector(request);
                var sw = Stopwatch.StartNew();

                for(int i = 0; i < candles.Length; i++)
                {
                    _token.ThrowIfCancellationRequested();
                    Fill? chosenFill = null;
                    OrderLimit? order = null;
                    PairOrder? pairOrder = null;
                    MarketEvent? marketEvent = null;
                    foreach (var pair in orderManager.PairOrders)
                    {

                        if (exec.TryFill(pair, in candles[i], out var fill) && portfolio.CanApplyFill(fill))
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
                    statistics.GetStatOnCandle(portfolio, in candles[i], marketEvent);
                }
                sw.Stop();
                _logger.LogInformation("Backtest run completed in {ElapsedSeconds} seconds. Final equity: {FinalEquity:F2}",
                    sw.Elapsed.TotalSeconds, portfolio.Equity(candles[^1].Close));
                return statistics.Build();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw;
            }     
        }

        public async Task<(List<BacktestStats> backtests, double timer)> RunBacktest(BacktestBatchRequest batchRequest, CancellationToken _token)
        {
            try
            {
                var swDb = Stopwatch.StartNew();
                var candles = await _klinesUploadFromDbService.GetKlinesForEngineAsync(batchRequest.RangeData, _token);
                swDb.Stop();
                if (candles.Length == 0) throw new InvalidOperationException("No candles loaded.");

                var results = new List<BacktestStats>();

                var swEngine = Stopwatch.StartNew();
                
                var runs = batchRequest.Runs
                .Select(run => Task.Run(() => Runner(candles, run, _token))).ToArray();
                var job = await Task.WhenAll(runs);
                
                foreach (var runResult in job)
                {
                    _logger.LogInformation("Backtest #{Id} completed.", runResult.Id);
                    results.Add(runResult);
                }
                swEngine.Stop();
                _logger.LogInformation($"Candles load time: {swDb.Elapsed.TotalSeconds:F2} seconds");
                _logger.LogInformation($"Backtest engine time: {swEngine.Elapsed.TotalSeconds:F2} seconds");
                
                return (results, swEngine.Elapsed.TotalSeconds);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw;
            }
            
        }

        public async Task<BacktestBatchResultEntity> BacktestRunnerBatchResult(BacktestBatchRequest batchRequest, CancellationToken _token)
        {
            var results = await RunBacktest(batchRequest, _token);
                return new BacktestBatchResultEntity
                {
                    Name = batchRequest.RangeData.Name,
                    RangeData = batchRequest.RangeData,
                    RunsCount = results.backtests.Count,
                    Symbol = batchRequest.RangeData.Symbol,
                    Interval = batchRequest.RangeData.Interval,
                    StartDate = batchRequest.RangeData.StartDate.ToUtcOrAssumeUtc(),
                    EndDate = batchRequest.RangeData.EndDate.ToUtcOrAssumeUtc(),
                    Stats = results.backtests,
                    ExecutionTime = results.timer
                };
        }
    }
}