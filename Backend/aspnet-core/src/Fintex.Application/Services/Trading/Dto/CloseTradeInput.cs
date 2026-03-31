using System;
using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Trading.Dto
{
    /// <summary>
    /// Input for closing an existing trade.
    /// </summary>
    public class CloseTradeInput
    {
        [Range(1, long.MaxValue)]
        public long TradeId { get; set; }

        public decimal? ExitPrice { get; set; }

        public DateTime? ClosedAt { get; set; }
    }
}
