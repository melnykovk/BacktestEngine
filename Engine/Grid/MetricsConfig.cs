using BacktestingEngine.Engine.Core;
using BacktestingEngine.DTO;
using BacktestingEngine.Engine;
using BacktestingEngine.Engine.MarketState;

namespace BacktestingEngine.Engine.Grid
{
    public sealed class MetricsConfig
    {
        private SmaConfig _smaConfig;
        public SmaConfig SmaConfig => _smaConfig;
        public MetricsConfig(SmaConfig smaConfig)
        {
            _smaConfig = smaConfig;
        }
    }



}