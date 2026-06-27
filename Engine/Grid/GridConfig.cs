using System.Diagnostics;
using GrindBotAPI.Engine.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace GrindBotAPI.Engine.Grid
{
    public class GridConfig
    {
        public int LevelsPerSide { get; set; }  // e.g. 10 levels above and 10 levels below the center price
        public decimal Step { get; set; } // e.g. 0.001m = 0.1%
        public decimal OrderSizeQuote { get; set; } // in quote currency, e.g. USDT

        public GridConfig(int levelsPerSide, decimal step, decimal orderSizeQuote)
        {
            if (levelsPerSide <= 0) throw new ArgumentOutOfRangeException(nameof(levelsPerSide));
            if (step <= 0 || step >= 0.5m) throw new ArgumentOutOfRangeException(nameof(step));
            if (orderSizeQuote <= 0) throw new ArgumentOutOfRangeException(nameof(orderSizeQuote));

            LevelsPerSide = levelsPerSide;
            Step = step;
            OrderSizeQuote = orderSizeQuote;
        }
    }

    public sealed class GridStrategy
    {
        private List<GridLimitLevel> _levels = new();
        private readonly GridConfig _cfg;
        public IReadOnlyList<GridLimitLevel> Levels => _levels;
        private decimal _centerPrice;
        public GridStrategy(GridConfig cfg, decimal centerPrice)
        {
            _cfg = cfg;
            _centerPrice = centerPrice;
        }

        private IReadOnlyList<GridLimitLevel> BuildGeometricalLines(decimal centerPrice)
        {
            _levels.Clear();

            var buy = centerPrice;     // building prices around the center price

            var downMul = 1m - _cfg.Step;       // buy
            var upMul = 1m + _cfg.Step;         // sell
            if (downMul <= 0) throw new InvalidOperationException("Step must be less than 1.");

            for (int i = 1; i <= _cfg.LevelsPerSide; i++)
            {
                buy *= downMul;    // buy
                var sell = buy * upMul;        // sell
                _levels.Add(new GridLimitLevel() { BuyPrice = buy, SellPrice = sell });
            }
            return Levels;
        }
        public GridMode Build()
        {
            BuildGeometricalLines(_centerPrice);
            var grid = new GridMode(_cfg, _levels);
            return grid;
        }
    }

    public sealed class GridMode
    {
        public GridConfig Config { get; }
        public IReadOnlyList<GridLimitLevel> Levels { get; }
        public GridMode(GridConfig config, IReadOnlyList<GridLimitLevel> levels)
        {
            Config = config;
            Levels = levels;
        }
    }
}