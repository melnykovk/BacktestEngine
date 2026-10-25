namespace BacktestingEngine.Entity
{
    public class KlineDatasetEntity
    {
        public string Symbol { get; set; } = null!;
        public string Interval { get; set; } = null!;
        public long FirstOpenTimeMs { get; set; }
        public long LastOpenTimeMs { get; set; }
        public int CandlesCount { get; set; }
    }
}