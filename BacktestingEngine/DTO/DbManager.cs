
namespace BacktestingEngine.DTO
{
    public sealed class DbCandleSummaryDto
    {
        public string Symbol { get; set; } = "";
        public string Interval { get; set; } = "";
        public long FirstOpenTimeMs { get; set; }
        public long LastOpenTimeMs { get; set; }
        public long CandlesCount { get; set; }
    }

    public sealed class DbUploadKlinesRequestDto
    {
        public string Symbol { get; set; } = "";
        public string Interval { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public sealed class DbUploadKlinesResponseDto
    {
        public string Symbol { get; set; } = "";
        public string Interval { get; set; } = "";
        public string RequestedRange => $"{FirstCandleTime:O} - {LastCandleTime:O}";
        public int UpdatedExistingCandles { get; set; }
        public int AddedNewCandles { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime FirstCandleTime { get; set; }
        public DateTime LastCandleTime { get; set; }

    }
}

