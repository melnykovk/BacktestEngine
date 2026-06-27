namespace GrindBotAPI.Engine.Core;

public sealed class Portfolio
{
    public decimal QuoteBalance { get; private set; } // USDT
    public decimal BaseBalance { get; private set; }  // BTC
    // public decimal FeesPaidQuote { get; private set; } // fees, in USDT

    public Portfolio(decimal startingQuote)
    {
        if (startingQuote <= 0) throw new ArgumentOutOfRangeException(nameof(startingQuote));
        QuoteBalance = startingQuote;
    }

    public void ApplyFill(Fill f)
    {
        // Buy: потратили quote + fee, получили base
        // Sell: отдали base, получили quote - fee
        if (f.Side == Side.Buy)
        {
            var cost = (f.BaseQty * f.Price);
            QuoteBalance -= (cost + f.FeeQuote);
            BaseBalance += f.BaseQty;
        }
        else
        {
            var proceeds = (f.BaseQty * f.Price);
            BaseBalance -= f.BaseQty;
            QuoteBalance += (proceeds - f.FeeQuote);
        }
    }

    public bool CanApplyFill(Fill? f)
    {
        if (f == null)
        {
            return false;
        }
        if (f?.Side == Side.Buy)
        {
            var cost = (f.BaseQty * f.Price) + f.FeeQuote;
            return QuoteBalance >= cost;
        }

        var sellQty = f?.BaseQty;
        return BaseBalance >= sellQty;
    }


    public decimal Equity(decimal lastPrice) => QuoteBalance + BaseBalance * lastPrice;
}