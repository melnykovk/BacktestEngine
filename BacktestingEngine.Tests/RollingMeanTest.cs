using Xunit;
using BacktestingEngine.Engine.MarketState;

public class RollingMeanTests
{
    [Fact]
    public void CalcAveragePerPriod()
    {
        var rm = new RollingMean(4);
        rm.Add(60000m);
        rm.Add(61000m);
        rm.Add(62000m);
        var result = rm.Add(63000m);
        Assert.Equal(61500m, result);
    }

    [Fact]
    public void NullReturnCheck()
    {
        var rm = new RollingMean(4);
        rm.Add(60000m);
        rm.Add(61000m);
        var result = rm.Add(62000m);
        Assert.Null(result);
    }

    [Fact]
    public void InvalidValueExceptionCheck()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RollingMean(0));
    }

    [Fact]
    public void RollingCheck()
    {
        var rm = new RollingMean(3);
        rm.Add(1m);
        rm.Add(2m);
        rm.Add(3m);
        rm.Add(4m);
        var result = rm.Add(5m);
        Assert.Equal(4m, result);
    }
}