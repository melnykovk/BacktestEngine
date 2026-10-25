using BacktestingEngine.Engine.Core;
using BacktestingEngine.Engine.Grid;
using BacktestingEngine.Migrations;
using CryptoExchange.Net.SharedApis;

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

    public sealed class BacktestResultResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
        public string Interval { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RunsCount{ get; set; }
        public double ExecutionTime { get; set; }
        public DateTime CreatedAtUtc { get; init; }
    }

    public sealed class BacktestResultDeleteResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
        public string Interval { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RunsCount{ get; set; }
    }

    public sealed class BacktestResultDeleteRequestDto
    {
        public Guid Id { get; set; }
    }

    public sealed class RangeBacktestData
    {
        public string Name {get;init;} = default!;
        public string Symbol { get; init; } = default!;
        public string Interval { get; init; } = default!;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}