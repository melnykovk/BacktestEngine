using Binance.Net.Interfaces;
using GrindBotAPI.DTO;
using GrindBotAPI.Entity;
using Microsoft.AspNetCore.SignalR;

namespace GrindBotAPI.DTO
{
    public record struct EngineCandle(
     long OpenTime,
     decimal Open,
     decimal High,
     decimal Low,
     decimal Close,
     decimal Volume,
     long CloseTime)
    {

    }
    public record KlineDTO(
    long OpenTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    long CloseTime,
    decimal QuoteAssetVolume,
    long NumberOfTrades,
    decimal TakerBuyBaseAsset,
    decimal TakerBuyQuoteAsset
)
    {

    }
    public class TradeDto
    {
        public string Symbol { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public DateTime TradeTime { get; set; }
    }


    public class TradesHub : Hub { }
}