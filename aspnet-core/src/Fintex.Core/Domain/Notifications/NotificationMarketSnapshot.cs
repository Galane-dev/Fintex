namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Lightweight market snapshot used to evaluate notification rules on each live update.
    /// </summary>
    public class NotificationMarketSnapshot
    {
        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; }

        public decimal Price { get; set; }

        public decimal? Bid { get; set; }

        public decimal? Ask { get; set; }

        public MarketVerdict Verdict { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public decimal? TrendScore { get; set; }
    }
}
