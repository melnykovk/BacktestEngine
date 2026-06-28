
namespace BacktestingEngine.Engine.MarketState
{
    public sealed class SmaConfigDto
    {
        public SmaSetupDTO? Buy { get; init; }
        public SmaSetupDTO? Sell { get; init; }
    }
    public sealed class SmaSetupDTO
        {
            public string Interval {get; set;} = null!;             // Type of candle.
            public int Period { get; init; }                         // Quantity of candles.
        }
}
