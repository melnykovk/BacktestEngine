using BacktestingEngine.DTO;

namespace BacktestingEngine.Engine
{
    public enum KlineInterval
    {
        s1 = 1,
        m1 = 60,
        m5 = 300,
        m15 = 900,
        m30 = 1800,
        h1 = 3600,
        h2 = 7200,
        h3 = 10800,
        h4 = 14400,
        h6 = 21600,
        h12 = 43200,
        d1 = 86400
    }


    public static class Timeframe
    {
        private static readonly Dictionary<string, KlineInterval> _map = new()
        {
            { "1s", KlineInterval.s1 },
            { "1m", KlineInterval.m1 },
            { "5m", KlineInterval.m5 },
            { "15m", KlineInterval.m15 },
            { "30m", KlineInterval.m30 },
            { "1h", KlineInterval.h1 },
            { "2h", KlineInterval.h2 },
            { "3h", KlineInterval.h3 },
            { "4h", KlineInterval.h4 },
            { "6h", KlineInterval.h6 },
            { "12h", KlineInterval.h12 },
            { "1d", KlineInterval.d1 }
        };

        public static KlineInterval Parse(string interval) =>
            _map.TryGetValue(interval, out var result)
                ? result
                : throw new ArgumentException($"Unknown interval: {interval}");


        public static int CalculateRequiredBaseCandles(KlineInterval metricInterval, KlineInterval baseInterval)
        {
            if ((int)metricInterval < (int)baseInterval)
                throw new InvalidOperationException("Metric interval cannot be smaller than base interval.");
            if((int)metricInterval % (int)baseInterval != 0)
                throw new InvalidOperationException("Metric interval must be divisible by base interval.");
            return (int)metricInterval / (int)baseInterval;
        }
    }


    public sealed class TimeframeAggregator
    {
        private readonly int _baseCandlesPerMetricCandle;
        private int _count;
        private decimal _open, _high, _low, _close, _volume;
        private long _openTime;

        public TimeframeAggregator(int BaseCandlesPerMetricCandle)
        {
            _baseCandlesPerMetricCandle = BaseCandlesPerMetricCandle;
        }
        public EngineCandle? AggregateCandleForMetric(EngineCandle engineCandle)
        {
            
            if(_count == 0)
            {
                _open = engineCandle.Open; 
                _high = engineCandle.High; 
                _low = engineCandle.Low; 
                _openTime = engineCandle.OpenTime; 
                _volume = engineCandle.Volume;
            }
            else
            {
                if (engineCandle.High > _high)
                     _high = engineCandle.High;

                if (engineCandle.Low < _low) 
                    _low = engineCandle.Low;

                _volume =+ engineCandle.Volume;
            }
            _close = engineCandle.Close;
            _count++;

            if(_count < _baseCandlesPerMetricCandle) 
                return null;
            _count = 0;

            return new EngineCandle(_openTime, _open, _high, _low, _close, _volume, engineCandle.CloseTime);
        }
    }
}