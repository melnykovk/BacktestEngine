namespace GrindBotAPI.Entity
{
    public class KlineEntity
    {
        public string Symbol { get; set; } = null!;
        public string Interval { get; set; } = null!;

        public long OpenTimeMs { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public long CloseTimeMs { get; set; }
        public decimal QuoteAssetVol { get; set; }
        public long TradesCount { get; set; }
        public decimal TakerBuyBase { get; set; }
        public decimal TakerBuyQuote { get; set; }
    }
}