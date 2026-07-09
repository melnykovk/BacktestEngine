using BacktestingEngine.Engine.Core;

public class PortfolioTest
{
    [Fact]
    public void StartBalanceCheck()
    {
        var pt = new Portfolio(1000m);
        var balance = pt.QuoteBalance;

        Assert.Equal(1000m, balance);
    }

    [Fact]
    public void StartBalanceNullCheck()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Portfolio(0));
    }

}