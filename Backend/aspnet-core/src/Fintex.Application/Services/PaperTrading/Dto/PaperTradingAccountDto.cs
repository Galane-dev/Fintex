using System;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Account summary for the paper trading simulator.
    /// </summary>
    public class PaperTradingAccountDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BaseCurrency { get; set; }

        public decimal StartingBalance { get; set; }

        public decimal CashBalance { get; set; }

        public decimal Equity { get; set; }

        public decimal RealizedProfitLoss { get; set; }

        public decimal UnrealizedProfitLoss { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastMarkedToMarketAt { get; set; }
    }
}
