using BacktestingEngine.DTO;
using BacktestingEngine.Engine.Core;

public sealed class BacktestBatchResultEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string? Name { get; set; }
        public RangeBacktestData RangeData { get; init; } = default!;
        public int RunsCount { get; set; }
        public string Symbol { get; set; } = default!;
        public string Interval { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<BacktestStats> Stats { get; init; } = new();
        public double ExecutionTime { get; set; }
        public DateTime CreatedAtUtc {get; init;} = DateTime.UtcNow;
    }