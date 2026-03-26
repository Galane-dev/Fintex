using System.Collections.Generic;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Aggregated paper trading dashboard snapshot for the current user.
    /// </summary>
    public class PaperTradingSnapshotDto
    {
        public PaperTradingAccountDto Account { get; set; }

        public List<PaperPositionDto> Positions { get; set; } = new List<PaperPositionDto>();

        public List<PaperOrderDto> RecentOrders { get; set; } = new List<PaperOrderDto>();

        public List<PaperTradeFillDto> RecentFills { get; set; } = new List<PaperTradeFillDto>();
    }
}
