using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;

namespace BacktestingEngine.DTO
{
    public sealed class BacktestBatchRequest
    {
        public RangeBacktestData RangeData { get; init; } = default!;
        public List<BacktestRunRequest> Runs { get; init; } = new();
    }

    public sealed class BacktestRunRequest
    {
        public string Id { get; init; } = default!;
        public RangeBacktestData RangeData { get; init; } = default!;
        public decimal StartingQuote { get; init; }
        public GridConfig GridConfig { get; init; } = default!;
        public decimal FeeRate { get; init; }
    }

    public sealed class BacktestRunResult
    {
        public string Id { get; init; } = default!;
        public BacktestStats Stats { get; init; } = default!;
    }
    public sealed class RangeBacktestData
    {
        public string Symbol { get; init; } = default!;
        public string Interval { get; init; } = default!;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

}