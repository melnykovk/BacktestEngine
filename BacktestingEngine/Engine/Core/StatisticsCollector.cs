using BacktestingEngine.DTO;
using BacktestingEngine.Engine.Grid;

namespace BacktestingEngine.Engine.Core
{
    public class StatisticsCollector
    {
        // Parameters
        private string _id;
        private string _symbol;
        private string _interval;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _startingQuote;
        private GridConfig _gridConfig;
        private decimal _feeRate;

        // Candle count
        private int _candleCount;

        // Equity results
        private readonly decimal _startEquity;
        private decimal _currentEquity;
        private decimal _peakEquity;
        private decimal _endEquity;
        private decimal _profitQuote;
        private decimal _profitPct;

        // Trading activity
        private int _tradesCount;
        private int _buyTradesCount;
        private int _sellTradesCount;
        private int _closedCyclesCount;
        private decimal _profitPerCyclePct;


        // Fees
        private decimal _totalFeesQuote;
        private decimal _feesToGrossProfitRatioPct;
        private decimal _feesLowestQuote;
        private decimal _feesHighestQuote;


        // Drawdown / risk
        private decimal _maxDrawdown;
        private decimal _maxDrawdownPct;
        private decimal _worstEquity;


        // Inventory / capital usage
        private decimal _minQuoteBalance;
        private decimal _currentBaseValue;
        private decimal _currentQouteValue;
        private decimal _maxBaseValueQuote;
        private decimal _maxBaseValueRelativeToStartPct;
        private decimal _maxCapitalInAssetPct;


        // Grid-specific
        private int _stuckPairsCount;
        private int _maxStuckPairs;
        private long _averageCycleDurationMs;
        private long _medianCycleDurationMs;
        private long _maxCycleDurationMs;
        private List<long> _cycleDurations = [];


        // Base price action
        public decimal _firstCandlePrice;
        public decimal _lastCandlePrice;
        public decimal _lowestPrice;
        public decimal _highestPrice;
        public decimal _maxGrowthPct;
        public decimal _maxDropPct;
        public decimal _maxPriceRangePercentage;
        public decimal _maxQuoteRangePerCandle;
        public decimal _maxQuoteRangePerCandlePct;

        // Other statistics
        private int _timeUnderWater;  // max time between low and high portfolio equity;


        public StatisticsCollector(BacktestRunRequest request)
        {
            _startEquity = request.StartingQuote;
            _currentEquity = request.StartingQuote;
            _peakEquity = request.StartingQuote;
            _worstEquity = request.StartingQuote;

            // Parameters
            _id = request.Id;
            _symbol = request.RangeData.Symbol;
            _interval = request.RangeData.Interval;
            _startDate = request.RangeData.StartDate;
            _endDate = request.RangeData.EndDate;
            _startingQuote = request.StartingQuote;
            _gridConfig = request.GridConfig;
            _feeRate = request.FeeRate;
        }
        public void GetStatOnCandle(
        Portfolio portfolio,
        EngineCandle engineCandle,
        MarketEvent? marketEvent)
        {
            // All stats are collected from the portfolio/strategy state
            // after fill application for the current candle.


            // Snapshot after fill processing, so we can analyze how fill influenced stats.
            UpdateEquityOnCandle(portfolio, engineCandle);
            UpdateBaseOnCandle(portfolio, engineCandle);
            UpdateEndEquity();

            // Drawdown and risk
            UpdateDrawdown();
            UpdateMaxCapitalInAssetPct();
            UpdateProfit();

            // Event / strategy metrics
            UpdateOnFill(marketEvent?.Fill);
            UpdateStackPairs(marketEvent);
            UpdateClosedCycles(marketEvent);
            UpdateProfitPerCycle();

            // Market action
            UpdatePriceCandle(engineCandle);
            UpdateMaxPriceRangeBetweenCandles(engineCandle);
        }
        private void UpdateEquityOnCandle(Portfolio portfolio, EngineCandle engineCandle)
        {
            _currentEquity = (portfolio.BaseBalance * engineCandle.Close) + portfolio.QuoteBalance;
            _currentQouteValue = portfolio.QuoteBalance;
            if (_currentEquity > _peakEquity)
            {
                _peakEquity = _currentEquity;
            }
            if (_currentEquity < _worstEquity)
            {
                _worstEquity = _currentEquity;
            }
            if (portfolio.QuoteBalance < _minQuoteBalance || _minQuoteBalance == 0)
            {
                _minQuoteBalance = portfolio.QuoteBalance;
            }
        }
        public void UpdateBaseOnCandle(Portfolio portfolio, EngineCandle engineCandle)
        {
            _currentBaseValue = (portfolio.BaseBalance * engineCandle.Close);
            if (_currentBaseValue > _maxBaseValueQuote)
            {
                _maxBaseValueQuote = _currentBaseValue;
                _maxBaseValueRelativeToStartPct = _maxBaseValueQuote / _startEquity * 100m;
            }
        }
        private void UpdateDrawdown()
        {
            var drawdown = _peakEquity - _currentEquity;
            if (drawdown > _maxDrawdown)
            {
                _maxDrawdown = drawdown;
                _maxDrawdownPct = ((_peakEquity - _currentEquity) / _peakEquity) * 100m;
            }
        }
        private void UpdateOnFill(Fill? fill)
        {
            if (fill == null)
            {
                return;
            }
            _tradesCount++;

            if (fill.Side == Side.Buy)
                _buyTradesCount++;

            else
                _sellTradesCount++;

            _totalFeesQuote += fill.FeeQuote;
            if (fill.FeeQuote < _feesLowestQuote || _feesLowestQuote == 0)
            {
                _feesLowestQuote = fill.FeeQuote;
            }
            if (fill.FeeQuote > _feesHighestQuote)
            {
                _feesHighestQuote = fill.FeeQuote;
            }

        }
        public void UpdateMaxCapitalInAssetPct()
        {
            var maxCapitalInAssetPct = _currentBaseValue / _currentEquity * 100m;
            if (maxCapitalInAssetPct > _maxCapitalInAssetPct)
            {
                _maxCapitalInAssetPct = maxCapitalInAssetPct;
            }
        }
        public void UpdateStackPairs(MarketEvent? marketEvent)
        {
            if (marketEvent == null)
            {
                return;
            }

            if (marketEvent.Pair.State == PairState.SellActive)
            {
                _stuckPairsCount++;
            }
            if (marketEvent.Pair.State == PairState.BuyActive)
            {
                _stuckPairsCount--;
            }

            if (_stuckPairsCount > _maxStuckPairs)
            {
                _maxStuckPairs = _stuckPairsCount;
            }
        }
        public void UpdateClosedCycles(MarketEvent? marketEvent)
        {
            if (marketEvent == null)
            {
                return;
            }
            if (marketEvent.NewOrder.Side == Side.Sell && marketEvent.NewOrder.Status == OrderStatus.Filled)
            {
                _closedCyclesCount++;
            }

            if (marketEvent.Pair != null && marketEvent.Pair.CloseTime > marketEvent.Pair.OpenTime && marketEvent.Pair.OpenTime > 0)
            {
                var durationMs = marketEvent.Pair.CloseTime - marketEvent.Pair.OpenTime;
                _cycleDurations.Add(durationMs);
                _averageCycleDurationMs = (long)_cycleDurations.Average();

                long median;
                var sortedDurations = _cycleDurations.OrderBy(x => x).ToArray();
                if (sortedDurations.Length % 2 == 0)
                {
                    median = (sortedDurations[sortedDurations.Length / 2 - 1] + sortedDurations[sortedDurations.Length / 2]) / 2;
                }
                else
                {
                    median = sortedDurations[sortedDurations.Length / 2];
                }
                _medianCycleDurationMs = median;

                if (durationMs > _maxCycleDurationMs)
                {
                    _maxCycleDurationMs = durationMs;
                }
            }
        }
        public void UpdatePriceCandle(EngineCandle engineCandle)
        {
            _candleCount++;
            if (_candleCount == 1)
            {
                _firstCandlePrice = engineCandle.Open;
                _lowestPrice = engineCandle.Low;
                _highestPrice = engineCandle.High;
            }

            _lastCandlePrice = engineCandle.Close;

            if (_lowestPrice > engineCandle.Low)
            {
                _lowestPrice = engineCandle.Low;
            }

            if (_highestPrice < engineCandle.High)
            {
                _highestPrice = engineCandle.High;
            }
            _maxPriceRangePercentage = (_highestPrice - _lowestPrice) / _lowestPrice * 100;

            var MaxGrowthPct = (_highestPrice - _firstCandlePrice) / _firstCandlePrice * 100m;
            var MaxDropPct = (_lowestPrice - _firstCandlePrice) / _firstCandlePrice * 100m;
            if (_maxGrowthPct < MaxGrowthPct)
            {
                _maxGrowthPct = MaxGrowthPct;
            }
            if (_maxDropPct > MaxDropPct)
            {
                _maxDropPct = MaxDropPct;
            }
        }
        public void UpdateEndEquity()
        {
            _endEquity = _currentEquity;
        }
        public void UpdateProfit()
        {
            _profitQuote = _endEquity - _startEquity;
            _profitPct = (_endEquity - _startEquity) / _startEquity * 100;
        }
        public void UpdateProfitPerCycle()
        {
            if (_closedCyclesCount != 0)
            {
                _profitPerCyclePct = _profitPct / _closedCyclesCount;
            }

        }
        public void UpdateMaxPriceRangeBetweenCandles(EngineCandle engineCandle)
        {
            decimal count;
            count = engineCandle.High - engineCandle.Low;
            if (count > _maxQuoteRangePerCandle)
            {
                _maxQuoteRangePerCandle = count;
                _maxQuoteRangePerCandlePct = (engineCandle.High - engineCandle.Low) / engineCandle.Low * 100m;
            }
        }
        public BacktestStats Build()
        {
            return new BacktestStats
            {

                // Parameters
                Id = _id,
                Symbol = _symbol,
                Interval = _interval,
                StartDate = _startDate,
                EndDate = _endDate,
                StartingQuote = _startingQuote,
                GridConfig = _gridConfig,
                FeeRate = _feeRate,

                // Equity*
                StartEquity = _startEquity,
                EndEquity = _endEquity,
                CurrentEquity = _currentEquity,
                PeakEquity = _peakEquity,
                ProfitQuote = _profitQuote,
                ProfitPct = _profitPct,

                // Trades*
                TradesCount = _tradesCount,
                BuyTradesCount = _buyTradesCount,
                SellTradesCount = _sellTradesCount,
                ClosedCyclesCount = _closedCyclesCount,
                ProfitPerCyclePct = _profitPerCyclePct,

                // Fees*
                TotalFeesQuote = _totalFeesQuote,
                FeesLowestQuote = _feesLowestQuote,
                FeesHighestQuote = _feesHighestQuote,
                FeesToGrossProfitRatioPct = _feesToGrossProfitRatioPct + (_profitQuote != 0 ? _totalFeesQuote / _profitQuote : 0) * 100m,

                // Risk*
                MaxDrawdown = _maxDrawdown,
                MaxDrawdownPct = _maxDrawdownPct,
                WorstEquity = _worstEquity,
                // underwaterCandles need to add.

                // Inventory*
                MinQuoteBalance = _minQuoteBalance,
                CurrentBaseValue = _currentBaseValue,
                CurrentQuoteValue = _currentQouteValue,
                MaxBaseValueQuote = _maxBaseValueQuote,
                MaxBaseValueRelativeToStartPct = _maxBaseValueRelativeToStartPct,
                MaxCapitalInAssetPct = _maxCapitalInAssetPct,

                // Grid-specific*
                StuckPairsCount = _stuckPairsCount,
                MaxStuckPairs = _maxStuckPairs,
                AverageCycleDurationMs = _averageCycleDurationMs,
                MedianCycleDurationMs = _medianCycleDurationMs,
                MaxCycleDurationMs = _maxCycleDurationMs,

                // Base price action*
                FirstCandlePrice = _firstCandlePrice,
                LastCandlePrice = _lastCandlePrice,
                LowestPrice = _lowestPrice,
                HighestPrice = _highestPrice,
                MaxGrowthPct = _maxGrowthPct,
                MaxDropPct = _maxDropPct,
                MaxPriceRangePercentage = _maxPriceRangePercentage,
                MaxQuoteRangePerCandle = _maxQuoteRangePerCandle,
                MaxQuoteRangePerCandlePct = _maxQuoteRangePerCandlePct

            };
        }

    }
}