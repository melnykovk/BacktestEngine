using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.DTO;

namespace BacktestingEngine.Engine.MarketState
{
    public sealed class MarketContext
    {
        public MetricsConfig MetricsConfig;
        public RangeBacktestData RangeBacktestData;

        public MarketContext(RangeBacktestData data, MetricsConfig metricsConfig)
        {
            MetricsConfig = metricsConfig;
            RangeBacktestData = data;
        }






        

    }
}
