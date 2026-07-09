using BacktestingEngine.Engine.Grid;

public class GridConfigTest
{
    [Fact]
    public void GridBuilderBuyLevelTest()
    {
        var gridC = new GridConfig(1, 0.01m, 50);
        var gridS = new GridStrategy(gridC, 100000m);
        var gridM = gridS.Build();

        var buy = gridM.Levels[0].BuyPrice;
        Assert.Equal(99000m, buy);
    }
    [Fact]
    public void GridBuilderSellLevelTest()
    {
        var gridC = new GridConfig(1, 0.01m, 50);
        var gridS = new GridStrategy(gridC, 100000m);
        var gridM = gridS.Build();

        var sell = gridM.Levels[0].SellPrice;
        Assert.Equal(99990m, sell);
    }

    [Fact]
    public void GridBuilderSellLevel5Test()
    {
        var gridC = new GridConfig(4, 0.01m, 50);
        var gridS = new GridStrategy(gridC, 100000m);
        var gridM = gridS.Build();

        var sell = gridM.Levels[3].SellPrice;
        Assert.Equal(97020.19701m, sell);
    }
}