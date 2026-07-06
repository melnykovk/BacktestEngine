using BacktestingEngine.Engine.Grid;

namespace BacktestingEngine.Engine.Core
{
    public sealed class BacktestStats
    {
        // Parameters of the run
        public string Id { get; init; } = default!;
        public string Symbol { get; init; } = default!;
        public string Interval { get; init; } = default!;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal StartingQuote { get; init; }
        public GridConfig GridConfig { get; init; } = default!;
        public decimal FeeRate { get; init; }

        // Equity results
        public decimal StartEquity { get; init; }
        public decimal EndEquity { get; init; }
        public decimal CurrentEquity { get; init; }
        public decimal PeakEquity { get; init; }
        public decimal ProfitQuote { get; init; }
        public decimal ProfitPct { get; init; }

        // Trading activity
        public int TradesCount { get; init; }
        public int BuyTradesCount { get; init; }
        public int SellTradesCount { get; init; }
        public int ClosedCyclesCount { get; init; }
        public decimal ProfitPerCyclePct { get; init; }

        // Fees
        public decimal TotalFeesQuote { get; init; }
        public decimal FeesLowestQuote { get; init; }
        public decimal FeesHighestQuote { get; init; }
        public decimal FeesToGrossProfitRatioPct { get; init; }

        // Drawdown / risk
        public decimal MaxDrawdown { get; init; }
        public decimal MaxDrawdownPct { get; init; }
        public decimal WorstEquity { get; init; }
        public int MaxUnderwaterCandles { get; init; }

        // Inventory / capital usage
        public decimal MinQuoteBalance { get; init; }
        public decimal CurrentBaseValue { get; init; }
        public decimal CurrentQuoteValue { get; init; }
        public decimal MaxBaseValueQuote { get; init; }
        public decimal MaxBaseValueRelativeToStartPct { get; init; }
        public decimal MaxCapitalInAssetPct { get; init; }

        // Grid-specific
        public int StuckPairsCount { get; init; }
        public int MaxStuckPairs { get; init; }
        public long AverageCycleDurationMs { get; init; }
        public long MedianCycleDurationMs { get; init; }
        public long MaxCycleDurationMs { get; init; }
        public string AverageCycleDurationFormatted => FormatDuration(AverageCycleDurationMs);
        public string MedianCycleDurationFormatted => FormatDuration(MedianCycleDurationMs);
        public string MaxCycleDurationFormatted => FormatDuration(MaxCycleDurationMs);

        // Base price action
        public decimal FirstCandlePrice { get; init; }
        public decimal LastCandlePrice { get; init; }
        public decimal LowestPrice { get; init; }
        public decimal HighestPrice { get; init; }
        public decimal MaxGrowthPct { get; init; }
        public decimal MaxDropPct { get; init; }
        public decimal MaxPriceRangePercentage { get; init; }
        public decimal MaxQuoteRangePerCandle { get; init; }
        public decimal MaxQuoteRangePerCandlePct { get; init; }


        private static string FormatDuration(long ms)
        {
            return TimeSpan
                .FromMilliseconds(ms)
                .ToString(@"dd\.hh\:mm\:ss");
        }

    }

}