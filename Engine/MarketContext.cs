using BacktestingEngine.Enums;
using BacktestingEngine.DTO;


namespace BacktestingEngine.Engine
{
    public class MarketContext
    {
        private readonly string Symbol;
        private readonly string Interval;
        public EngineCandle? CurrentCandle { get; private set; }
        public EngineCandle? PreviousCandle { get; private set; }
        public int Index { get; private set; }


        public MarketContext(string symbol, string interval)
        {
            Symbol = symbol;
            Interval = interval;
            CurrentCandle = null;
            PreviousCandle = null;
            Index = 0;
        }

        public CandleStatus Advance(EngineCandle engineCandle)
        {
            if (CurrentCandle == null)
            {
                CurrentCandle = engineCandle;
                Index++;
                return CandleStatus.First;
            }
            if (engineCandle.OpenTime < CurrentCandle?.OpenTime)
            {
                return CandleStatus.OutOfOrder;
            }
            if (engineCandle.OpenTime == CurrentCandle?.OpenTime)
            {
                return CandleStatus.Duplicate;
            }
            if (engineCandle.OpenTime > CurrentCandle?.OpenTime)
            {
                PreviousCandle = CurrentCandle;
                CurrentCandle = engineCandle;
                Index++;
                return CandleStatus.Advanced;
            }
            else
            {
                throw new Exception("Unexpected case in Advance method");
            }

        }
    }
}