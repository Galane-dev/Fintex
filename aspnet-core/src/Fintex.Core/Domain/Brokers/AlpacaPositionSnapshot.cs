using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Flattened open position view returned from Alpaca.
    /// </summary>
    public class AlpacaPositionSnapshot
    {
        public string Symbol { get; set; }

        public string Side { get; set; }

        public decimal Quantity { get; set; }

        public decimal? AverageEntryPrice { get; set; }

        public decimal? CurrentPrice { get; set; }

        public decimal? MarketValue { get; set; }

        public decimal? UnrealizedProfitLoss { get; set; }

        public decimal? UnrealizedProfitLossPercent { get; set; }

        public string RawSummary()
        {
            return $"{Symbol} {Side} qty={Quantity} avg={AverageEntryPrice} current={CurrentPrice} upl={UnrealizedProfitLoss}";
        }
    }
}
