using BacktestingEngine.DTO;
using BacktestingEngine.Engine.Core;
public class ExecutionModelTest
{
    private static EngineCandle Candle(decimal low, decimal high, decimal close = 100m)
        => new EngineCandle(
            OpenTime: 1000,
            Open: 100m,
            High: high,
            Low: low,
            Close: close,
            Volume: 10m,
            CloseTime: 2000);


    private static OrderLimit BuyOrder(decimal Price, decimal QuoteAmount)
    => new OrderLimit(
        id: Guid.NewGuid(),
        side: Side.Buy,
        status: OrderStatus.Active,
        price: Price,
        quoteAmount: QuoteAmount,
        baseAmount: null
    );
    private static OrderLimit SellOrder(decimal Price, decimal QuoteAmount, decimal? BaseAmount)
    => new OrderLimit(
        id: Guid.NewGuid(),
        side: Side.Sell,
        status: OrderStatus.Active,
        price: Price,
        quoteAmount: QuoteAmount,
        baseAmount: BaseAmount
    );
    private static PairOrder OrderPair(OrderLimit sellOrder, OrderLimit buyOrder, PairState stateOfOrder, decimal? OpenBaseQty)
        => new PairOrder(
            pairId: 1,
            buy: buyOrder,
            sell: sellOrder,
            state: stateOfOrder,
            openBaseQty: OpenBaseQty,
            openTime: 0,
            closeTime: 0
        );
        
    [Fact]
    public void TryFillSellOrderTrueTest()
    {
        var candle = Candle(50000m, 60000m);
        var orderBuy = BuyOrder(50001m, 50m);
        var orderSell = SellOrder(55001m, 50m, 0.001m);
        var pair = OrderPair(orderSell, orderBuy, PairState.SellActive, orderSell.BaseAmount);

        var exModel = new ExecutionModel(0.0001m);           // 0.001% fee
        var filled = exModel.TryFill(pair, candle, out var fill);
        Assert.True(filled);
        Assert.NotNull(fill);
        Assert.Equal(55001m, fill!.Price);
    }

    [Fact] 
    public void TryFillSellOrderFalseTest()
    {
        var candle = Candle(50000m, 60000m);
        var orderBuy = BuyOrder(50001m, 50m);
        var orderSell = SellOrder(60001m, 50m, 0.001m);
        var pair = OrderPair(orderSell, orderBuy, PairState.SellActive, orderSell.BaseAmount);

        var exModel = new ExecutionModel(0.0001m);           // 0.001% fee
        var filled = exModel.TryFill(pair, candle, out var fill);
        Assert.False(filled);
        Assert.Null(fill);
    }

    [Fact]
    public void TryFillSellOrderWithoutBase()
    {
        var candle = Candle(50000m, 60000m);
        var orderBuy = BuyOrder(50001m, 50m);
        var orderSell = SellOrder(55001m, 50m, 0.001m);
        var pair = OrderPair(orderSell, orderBuy, PairState.SellActive, 0);
        var exModel = new ExecutionModel(0.0001m);           // 0.001% fee

        // var filled = exModel.TryFill(pair, candle, out var fill);
        // Assert.True(filled);
        Assert.Throws<InvalidOperationException>(()=> exModel.TryFill(pair, candle, out var fill));
    }

    [Fact]
    public void TryFillBuyOrderTrueTest()
    {
        var candle = Candle(50000m, 60000m);
        var orderBuy = BuyOrder(50001m, 50m);
        var orderSell = SellOrder(55001m, 50m, null);
        var pair = OrderPair(orderSell, orderBuy, PairState.BuyActive, null);

        var exModel = new ExecutionModel(0.0001m);           // 0.001% fee
        var filled = exModel.TryFill(pair, candle, out var fill);
        Assert.True(filled);
        Assert.NotNull(fill);
        Assert.Equal(50001m, fill!.Price);
    }

    [Fact]
    public void TryFillBuyOrderFalseTest()
    {
        var candle = Candle(50000m, 60000m);
        var orderBuy = BuyOrder(49999m, 50m);
        var orderSell = SellOrder(55001m, 50m, null);
        var pair = OrderPair(orderSell, orderBuy, PairState.BuyActive, null);

        var exModel = new ExecutionModel(0.0001m);           // 0.001% fee
        var filled = exModel.TryFill(pair, candle, out var fill);
        Assert.False(filled);
        Assert.Null(fill);
    }

}