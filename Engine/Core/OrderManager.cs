using BacktestingEngine.Engine.Grid;

namespace BacktestingEngine.Engine.Core;

public sealed class OrderManager
{
    private readonly List<OrderLimit> _orders = new();
    private readonly List<PairOrder> _pairOrders = new();
    private readonly GridMode _grid;
    public IReadOnlyList<OrderLimit> Orders => _orders;
    public IReadOnlyList<PairOrder> PairOrders => _pairOrders;
    public int _pairIdCount = 0;
    public int NextPairId() => ++_pairIdCount;

    public OrderManager(GridMode grid)
    {
        _grid = grid;
        BuildOrders();
    }
    public void BuildOrders()
    {
        _pairOrders.Clear();
        foreach (var level in _grid.Levels)
        {
            var buyOrder = new OrderLimit(Guid.NewGuid(), Side.Buy, OrderStatus.Active, level.BuyPrice, _grid.Config.OrderSizeQuote, null);
            var sellOrder = new OrderLimit(Guid.NewGuid(), Side.Sell, OrderStatus.Active, level.SellPrice, _grid.Config.OrderSizeQuote, null);
            var pairOrder = new PairOrder(NextPairId(), buyOrder, sellOrder, PairState.BuyActive, null);
            _pairOrders.Add(pairOrder);
        }
    }

    public OrderLimit? CanApplyOrder(Fill fill)
    {
        for (int i = 0; i < _pairOrders.Count; i++)
        {
            var pair = _pairOrders[i];

            if (pair.Buy.Id == fill.OrderId)
            {
                pair.Buy.Status = OrderStatus.Filled;
                pair.Sell.Status = OrderStatus.Active;      // activate sell order
                pair.Buy.Price = fill.Price;                // update buy price to fill price for accurate PnL calculation
                pair.Buy.QuoteAmount -= fill.QuoteAmount;   // update quote amount for partial fills
                pair.Sell.BaseAmount = fill.BaseQty;        // set base amount for sell order

                // Need to exclude some data from pair to avoid confusion in statistics
                pair.State = PairState.SellActive;
                pair.OpenBaseQty = fill.BaseQty;
                pair.OpenTime = fill.TradeTime;
                return pair.Buy;
            }

            if (pair.Sell.Id == fill.OrderId)
            {
                pair.Sell.Status = OrderStatus.Filled;
                pair.Buy.Status = OrderStatus.Active;       // activate buy order
                pair.Sell.Price = fill.Price;               // update sell price to fill price for accurate PnL calculation
                pair.Sell.BaseAmount -= fill.BaseQty;        // set base amount for sell order
                pair.Buy.QuoteAmount = _grid.Config.OrderSizeQuote;

                // Need to exclude some data from pair to avoid confusion in statistics
                pair.State = PairState.BuyActive;
                pair.OpenBaseQty = null;
                pair.CloseTime = fill.TradeTime;

                return pair.Sell;
            }
        }
        return null;
    }
}