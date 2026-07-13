namespace BacktestingEngine.Engine.Core;

public enum Side { Buy, Sell }
public enum PairState { BuyActive, SellActive }
public enum OrderStatus { Active, PartiallyFilled, Filled, Cancelled }

public sealed class GridLimitLevel
{
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
}

public sealed class PairOrder
{
    public int PairId { get; set; }
    public OrderLimit Buy { get; set; }
    public OrderLimit Sell { get; set; }
    public PairState State { get; set; }
    public decimal? OpenBaseQty { get; set; }
    public long OpenTime { get; set; } = 0;
    public long CloseTime { get; set; } = 0;

    public PairOrder(int pairId, OrderLimit buy, OrderLimit sell, PairState state, decimal? openBaseQty, long openTime = 0, long closeTime = 0)
    {
        PairId = pairId;
        Buy = buy;
        Sell = sell;
        State = state;
        OpenBaseQty = openBaseQty;
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public OrderLimit GetActiveOrder()
    {
        return State == PairState.BuyActive ? Buy : Sell;
    }
}

public sealed class OrderLimit
{
    public Guid Id { get; set; }
    public Side Side { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Price { get; set; }              // Btc price in usdt
    public decimal QuoteAmount { get; set; }        // Fix price in USDT
    public decimal? BaseAmount { get; set; }
    public OrderLimit(Guid id, Side side, OrderStatus status, decimal price, decimal quoteAmount, decimal? baseAmount)
    {
        Id = id;
        Side = side;
        Status = status;
        Price = price;
        QuoteAmount = quoteAmount;
        BaseAmount = baseAmount;
    }
}

public sealed record Fill(
    int PairId,
    Guid OrderId,
    Side Side,
    decimal BaseQty,            // Btc amount,   e.g. 0.001 btc
    decimal Price,              // Btc price in usdt,    e.g. 70000
    decimal FeeQuote,           // Fee in quote currency USDT, e.g. 0.01%
    decimal QuoteAmount,        // Quote amount in USDT, e.g. 50
    long TradeTime = 0          // Trade time counted on candle close time, for simplicity
);

public sealed record MarketEvent(
    PairOrder Pair,
    OrderLimit NewOrder,
    Fill Fill
);
