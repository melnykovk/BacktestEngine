using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.DTO;

namespace BacktestingEngine.Engine.MarketState
{
    public sealed class SmaConfig
    {
        public SmaSetup? SmaBuySetup { get; init; }
        public SmaSetup? SmaSellSetup { get; init; }
    }


    public sealed class SmaSetup
    {
        public KlineInterval Interval { get; init; }                           // Type of candle.
        public KlineInterval BaseInterval { get; init; }                       // Base type of candle
        public int Period { get; init; }                                       // Quantity of candles.
        public int BaseCandlesPerMetricCandle { get; init; }                   // Quantity of base candles for create 1 metric candle.
        public SmaSetup(SetupDTO setupDTO, RangeBacktestData data)
        {
            Interval = Timeframe.Parse(setupDTO.Interval);
            Period = setupDTO.Period;
            BaseInterval = Timeframe.Parse(data.Interval);
            BaseCandlesPerMetricCandle = Timeframe.CalculateRequiredBaseCandles(Interval, BaseInterval);
        }
    }


    public sealed class RollingMean
    {
        private readonly Queue<decimal> _buffer;                                // Buffer.
        private readonly int _period;                                           // Quantity of candles.
        private decimal _sum;                                                   // Summary of prices.
        
        public RollingMean(int period)
        {
            if (period <= 0) throw new ArgumentOutOfRangeException(nameof(period));
            _period = period;
            _buffer = new Queue<decimal>(period);
        }

        public decimal? Add(decimal value)
        {
            _buffer.Enqueue(value);
            _sum += value;

            if (_buffer.Count > _period)
                _sum -= _buffer.Dequeue(); 

            if (_buffer.Count < _period)
                return null;

            return _sum / _period;
        }
    }


    public sealed class SmaIndicator
    {
        private readonly RollingMean? _buy;
        private readonly RollingMean? _sell;
        public decimal? BuyValue { get; private set; }
        public decimal? SellValue { get; private set; }
        private readonly TimeframeAggregator? _timeframeAggregatorBuy;
        private readonly TimeframeAggregator? _timeframeAggregatorSell;

        public SmaIndicator(SmaConfig smaConfig)
        {
            if (smaConfig.SmaBuySetup != null)  _buy  = new RollingMean(smaConfig.SmaBuySetup.Period);
            if (smaConfig.SmaSellSetup != null) _sell = new RollingMean(smaConfig.SmaSellSetup.Period);
            
            if(smaConfig.SmaBuySetup != null) _timeframeAggregatorBuy = new TimeframeAggregator(smaConfig.SmaBuySetup.BaseCandlesPerMetricCandle);
            if(smaConfig.SmaSellSetup != null) _timeframeAggregatorSell = new TimeframeAggregator(smaConfig.SmaSellSetup.BaseCandlesPerMetricCandle);
        }
        
        public void Add(EngineCandle candle)
        {
            if (_buy != null)
            {
                var metricCandle = _timeframeAggregatorBuy!.AggregateCandleForMetric(candle);
                if (metricCandle != null)
                    BuyValue = _buy.Add(metricCandle.Value.Close);
            }
            if(_sell != null)
            {
                var metricCandle = _timeframeAggregatorSell!.AggregateCandleForMetric(candle);
                if (metricCandle != null)
                    SellValue = _sell.Add(metricCandle.Value.Close);
            }
        }
    }
}