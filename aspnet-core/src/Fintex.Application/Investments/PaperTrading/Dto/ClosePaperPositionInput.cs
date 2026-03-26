using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Closes all or part of an open paper position.
    /// </summary>
    public class ClosePaperPositionInput
    {
        [Range(1, long.MaxValue)]
        public long PositionId { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? ExitPrice { get; set; }
    }
}
