using BacktestingEngine.DTO;

namespace BacktestingEngine.Engine.Core;

public sealed class ExecutionModel
{
    private readonly decimal _feeRate; // 0.001m = 0.1%
    public ExecutionModel(decimal feeRate)
    {
        if (feeRate < 0 || feeRate > 0.01m) throw new ArgumentOutOfRangeException(nameof(feeRate));
        _feeRate = feeRate;
    }



    public bool TryFill(PairOrder pair, EngineCandle engineCandle, out Fill? fill)
    {
        fill = null;

        // Determine which order is active now (buy or sell)
        var activeOrder = pair.GetActiveOrder();

        // Check if the active order price was touched by the candle
        var touched =
            activeOrder.Side == Side.Buy
                ? engineCandle.Low <= activeOrder.Price
                : engineCandle.High >= activeOrder.Price;

        if (!touched) return false;

        decimal baseQty;
        if (activeOrder.Side == Side.Buy)
        {

            // Generate baseQty for buy order.
            baseQty = activeOrder.QuoteAmount / activeOrder.Price;
        }
        else
        {

            if (pair.OpenBaseQty is null || pair.OpenBaseQty <= 0)
                throw new InvalidOperationException("Sell fill attempted without open base quantity");
            baseQty = pair.OpenBaseQty.Value;
        }

        var feeQuote = (baseQty * activeOrder.Price) * _feeRate;
        var quoteAmount = activeOrder.Side == Side.Buy
            ? activeOrder.QuoteAmount
            : baseQty * activeOrder.Price;

        // Fill is always fully filled at the order price for simplicity. No partial fills, no slippage.
        fill = new Fill(
            pair.PairId,
            activeOrder.Id,
            activeOrder.Side,
            baseQty,
            activeOrder.Price,
            feeQuote,
            quoteAmount,
            engineCandle.CloseTime);
        return true;
    }
}