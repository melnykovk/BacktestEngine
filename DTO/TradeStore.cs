using System.Collections.Concurrent;
using Binance.Net.Objects.Models.Spot.Socket;


namespace BacktestingEngine.DTO
{
    public class TradeStore
    {
        // Для каждой пары храним очередь последних сделок
        private readonly ConcurrentDictionary<string, ConcurrentQueue<BinanceStreamTrade>> _trades = new();

        private readonly int _maxTrades; // сколько последних сделок хранить

        public TradeStore(int maxTrades = 100)
        {
            _maxTrades = maxTrades;
        }


        // Добавляем новую сделку
        public void AddTrade(string symbol, BinanceStreamTrade trade)
        {
            var queue = _trades.GetOrAdd(symbol, _ => new ConcurrentQueue<BinanceStreamTrade>());
            queue.Enqueue(trade);

            // Держим только последние _maxTrades элементов
            while (queue.Count > _maxTrades)
                queue.TryDequeue(out _);
        }


        // Получаем последнюю сделку для конкретной пары
        public BinanceStreamTrade? GetLastTrade(string symbol)
        {
            if (_trades.TryGetValue(symbol, out var queue))
                return queue.TryPeek(out var trade) ? trade : null;
            return null;
        }


        // При желании можно вернуть последние N сделок
        public BinanceStreamTrade[] GetLastNTrades(string symbol, int n)
        {
            if (_trades.TryGetValue(symbol, out var queue))
                return queue.ToArray().TakeLast(n).ToArray();
            return Array.Empty<BinanceStreamTrade>();
        }
    }
}