using GrindBotAPI.Engine.Core;
using GrindBotAPI.DTO;
using GrindBotAPI.Engine;
using GrindBotAPI.Engine.MarketState;

namespace GrindBotAPI.Engine.Grid
{
    public sealed class RulesConfig
    {
        private SmaConfig _smaConfig;
        public SmaConfig SmaConfig => _smaConfig;
        public RulesConfig(SmaConfig smaConfig)
        {
            _smaConfig = smaConfig;
        }
    }



}