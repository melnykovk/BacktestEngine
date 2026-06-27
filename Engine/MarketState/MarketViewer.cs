using GrindBotAPI.Engine.Core;
using GrindBotAPI.Engine.Grid;
using GrindBotAPI.DTO;

namespace GrindBotAPI.Engine.MarketState
{
    public class MarketContext
    {
        private RulesConfig _rulesConfig;
        private RangeBacktestData _rangeBacktestData;

        public MarketContext(RangeBacktestData data, RulesConfig rulesConfig)
        {
            _rulesConfig = rulesConfig;
            _rangeBacktestData = data;
        }






        public void BuildSMABuy(EngineCandle engineCandle)
        {
            if (_rulesConfig.SmaConfig.smaBuySetup != null)
            {
                var smaInterval = _rulesConfig.SmaConfig.smaBuySetup.Interval;
                var smaTf = Timeframe.Parse(smaInterval);
                var baseTf = Timeframe.Parse(_rangeBacktestData.Interval);
                if ((int)smaTf % (int)baseTf == 0)
                {

                }
            }




            if (_rulesConfig.SmaConfig.smaSellSetup != null)
            {

            }
        }
    }
}
