using System;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Normalized trade activity record used by behavioral analysis across live and paper trading.
    /// </summary>
    public class BehaviorTradeActivity
    {
        public string Source { get; set; }

        public string Symbol { get; set; }

        public AssetClass AssetClass { get; set; }

        public TradeDirection Direction { get; set; }

        public decimal Quantity { get; set; }

        public decimal EntryPrice { get; set; }

        public decimal? ExitPrice { get; set; }

        public decimal RealizedProfitLoss { get; set; }

        public string Status { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
