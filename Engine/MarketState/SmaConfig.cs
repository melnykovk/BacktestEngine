using GrindBotAPI.Engine.Core;
using GrindBotAPI.Engine.Grid;
using GrindBotAPI.DTO;

namespace GrindBotAPI.Engine.MarketState
{
    public class SmaConfig
    {
        public SmaSetup? smaBuySetup;
        public SmaSetup? smaSellSetup;

    }
    public class SmaSetup
    {
        public string Interval { get; set; } = null!;               // в свечах
        public decimal Threshold { get; init; }                     // порог slope для срабатывания
        public decimal Multiplier { get; init; }                    // на сколько расширять сетку  
    }
}