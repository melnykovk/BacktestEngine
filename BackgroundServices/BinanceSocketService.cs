using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot.Socket;
using Microsoft.AspNetCore.SignalR;
using BacktestingEngine.DTO;

namespace BacktestingEngine.BackgroundServices
{
    public class BinanceSocketService : BackgroundService
    {
        private readonly BinanceSocketClient _client;
        private readonly IHubContext<TradesHub> _hub;
        private readonly TradeStore _store;

        public BinanceSocketService(IHubContext<TradesHub> hub, TradeStore store)
        {
            _client = new BinanceSocketClient();
            _hub = hub;
            _store = store;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.SpotApi.ExchangeData.SubscribeToTradeUpdatesAsync("BTCUSDT", async data =>
            {
                _store.AddTrade("BTCUSDT", data.Data);              // сохраняем в store
                var dto = new TradeDto
                {
                    Symbol = data.Data.Symbol,
                    Price = data.Data.Price,
                    Quantity = data.Data.Quantity,
                    TradeTime = data.Data.TradeTime
                };
                await _hub.Clients.All.SendAsync("ReceiveTrade", dto);  // пушим в Hub
            });

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(500, stoppingToken);
        }


    }
}
