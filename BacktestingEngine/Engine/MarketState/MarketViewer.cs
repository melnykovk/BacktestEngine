using BacktestingEngine.Engine.Grid;
using BacktestingEngine.DTO;

namespace BacktestingEngine.Engine.MarketState
{
    public sealed class MarketViewer
    {
        public MetricsConfig MetricsConfig;
        public RangeBacktestData RangeBacktestData;

        public MarketViewer(RangeBacktestData data, MetricsConfig metricsConfig)
        {
            MetricsConfig = metricsConfig;
            RangeBacktestData = data;
        }
    }
}
