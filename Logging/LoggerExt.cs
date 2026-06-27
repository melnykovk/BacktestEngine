using Microsoft.Extensions.Logging;
using GrindBotAPI.Engine.Core;

namespace GrindBotAPI.Logging;

public static class LoggerExtensions
{
    public static void LogBacktestResult(this ILogger logger, BacktestStats result)
    {
        logger.LogInformation(
                """

                ======== BACKTEST RESULT ========
                Id: #{Id}
                
                Equity:
                    StartEquity:    {StartEquity}
                    EndEquity:  {EndEquity}
                    PeakEquity:     {PeakEquity}
                    ProfitQuote:    {ProfitQuote}
                    ProfitPct:      {ProfitPct}

                Trades:
                    Total:  {TradesCount}
                    Buy:    {BuyTradesCount}
                    Sell:   {SellTradesCount}
                    ClosedCyclesCount:  {ClosedCyclesCount}
                    ProfitPerCycle:     {ProfitPerCycle}

                Drawdown:
                    MaxDrawdown:    {MaxDrawdown}
                    MaxDrawdownPct: {MaxDrawdownPct}
                    WorstEquity:    {WorstEquity}
                    UnderwaterCandles:  need to add.

                Inventory:
                    MinQuoteBalance:    need to add.
                    CurrentBaseValue:   {CurrentBaseValue}
                    CurrentQuoteValue:  {CurrentQuoteValue}
                    MaxBaseValueQuote:  {MaxBaseValueQuote}
                    MaxBaseValuePct:    need to add.
                    MaxCapitalInAssetPct:   {MaxCapitalInAsset}

                Grid-specific:
                    StuckPairs: {StuckPairsCount}
                    Grid build price:   {FirstCandlePrice}.
                    AverageCycleDurationCandles:    need to add.
                    MaxCycleDurationCandles:    need to add.

                Base price action:
                    FirstCandlePrice:   {FirstCandlePrice}
                    LastCandlePrice:    {LastCandlePrice}
                    LowestPrice:    {LowestPrice}
                    HighestPrice:   {HighestPrice}
                    MaxGrowthPct:   {MaxGrowthPct}
                    MaxDropPct:     {MaxDropPct}
                    MaxPriceRangePct:   {MaxPriceRangePct}
                =================================
                """,
                // Id*
                result.Id,
                // Equity*
                result.StartEquity,
                result.EndEquity,
                result.PeakEquity,
                result.ProfitQuote,
                result.ProfitPct,

                // Trades*
                result.TradesCount,
                result.BuyTradesCount,
                result.SellTradesCount,
                result.ClosedCyclesCount,
                result.ProfitPerCyclePct,

                // Fees* should be here.

                // Drawdown*
                result.MaxDrawdown,
                result.MaxDrawdownPct,
                result.WorstEquity,
                // underwaterCandles need to add

                // Inventory*
                // minQuoteBalance need to add.
                result.CurrentBaseValue,
                result.CurrentQuoteValue,
                result.MaxBaseValueQuote,
                //maxBaseValuePct need to add
                result.MaxCapitalInAssetPct,

                //Grid-specific*
                result.StuckPairsCount,
                result.FirstCandlePrice,
                // averageCycleDurationCandles need to add.
                // maxCycleDurationCandles need to add.

                // Base price action*
                result.FirstCandlePrice,
                result.LastCandlePrice,
                result.LowestPrice,
                result.HighestPrice,
                result.MaxGrowthPct,
                result.MaxDropPct,
                result.MaxPriceRangePercentage
                );
    }
}