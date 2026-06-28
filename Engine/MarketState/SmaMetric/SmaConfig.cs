using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.DTO;

namespace BacktestingEngine.Engine.MarketState
{
    public sealed class SmaConfig
    {
        public SmaSetup? smaBuySetup;
        public SmaSetup? smaSellSetup;
    }
    public sealed class SmaSetup
    {
        public KlineInterval Interval;                           // Type of candle.
        public int Period { get; init; }                         // Quantity of candles.
        public SmaSetup(SmaSetupDTO smaSetupDTO)
        {
            Interval = Timeframe.Parse(smaSetupDTO.Interval);
            Period = smaSetupDTO.Period;
        }
    }

    public sealed class SmaWindow
    {
        private readonly EngineCandle[] _buffer;
        private int _count;
        private decimal _sum;
        public SmaWindow(SmaSetup setup)
        {
            _buffer = new EngineCandle[setup.Period];
        }
        public decimal Add(EngineCandle candle)
        {
            return _sum / _count;
        }
    }
    public sealed class SmaIndicator
    {
        private readonly SmaWindow? _buyWindow;
        private readonly SmaWindow? _sellWindow;
        public SmaIndicator(SmaConfig smaConfig)
        {
            if(smaConfig.smaBuySetup != null)
            _buyWindow = new SmaWindow(smaConfig.smaBuySetup);
                
            if(smaConfig.smaSellSetup != null)
                _sellWindow = new SmaWindow(smaConfig.smaSellSetup);
        }

        public void BuildSMABuy(EngineCandle engineCandle)
        {

        }
    }
}