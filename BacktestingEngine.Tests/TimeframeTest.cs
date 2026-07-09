using BacktestingEngine.Engine;
public class TimeframeTests
{
    [Fact]
    public void TimeframeMapCheck()
    {
        var result = Timeframe.CalculateRequiredBaseCandles(KlineInterval.h1, KlineInterval.m1);
        Assert.Equal(60, result);
    }

    [Fact]
    public void TimeframeRecognizeByStringCheck()
    {
        var result = Timeframe.Parse("4h");
        var recognize = Timeframe.CalculateRequiredBaseCandles(result, KlineInterval.h1);
        Assert.Equal(4, recognize);
    }
    [Fact]
    public void TimeframeParseUnknownIntervalCheck()
    {
        Assert.Throws<ArgumentException>(() => 
        Timeframe.Parse("9m"));
    }

    [Fact]
    public void CalculateInvalidParametersCheck()
    {
        Assert.Throws<InvalidOperationException>(() => 
        Timeframe.CalculateRequiredBaseCandles(KlineInterval.h1, KlineInterval.h4));
    }
    [Fact]
    public void CalculateInvalidDivideCheck()
    {
        Assert.Throws<InvalidOperationException>(() => 
        Timeframe.CalculateRequiredBaseCandles(KlineInterval.h4, KlineInterval.h3));
    }
}
    