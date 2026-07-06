
namespace BacktestingEngine.Engine.MarketState
{
    public sealed class SetupDTO
    {
        public string Interval { get; set;} = null!;             // Type of candle.
        public int Period { get; init; }                         // Quantity of candles.
    }
}
