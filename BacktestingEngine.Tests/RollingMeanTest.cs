using Xunit;
using BacktestingEngine.Engine.MarketState;

public class RollingMeanTests
{
    [Fact]
    public void Returns_Null_Until_Buffer_Is_Full()
    {
        var sma = new RollingMean(3);         

        Assert.Null(sma.Add(10m));             
        Assert.Null(sma.Add(20m));             
        Assert.NotNull(sma.Add(30m));          
    }

    [Fact]
    public void Computes_Correct_Average_When_Full()
    {
        var sma = new RollingMean(3);

        sma.Add(10m);
        sma.Add(20m);
        var result = sma.Add(30m);

        Assert.Equal(20m, result);
    }

    [Fact]
    public void Slides_Window_Dropping_Oldest()
    {
        var sma = new RollingMean(3);

        sma.Add(10m);
        sma.Add(20m);
        sma.Add(30m);
        var result = sma.Add(40m);

        Assert.Equal(30m, result);
    }

    [Fact]
    public void Throws_On_Invalid_Period()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RollingMean(0));
    }
}