using System;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Lightweight macro event displayed in trade recommendations.
    /// </summary>
    public class EconomicCalendarEventDto
    {
        public string Title { get; set; }

        public string Source { get; set; }

        public DateTime OccursAtUtc { get; set; }

        public decimal ImpactScore { get; set; }
    }
}
