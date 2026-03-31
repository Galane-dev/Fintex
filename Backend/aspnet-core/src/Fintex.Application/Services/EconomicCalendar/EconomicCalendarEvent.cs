using System;

namespace Fintex.Investments.EconomicCalendar
{
    /// <summary>
    /// Represents an upcoming high-impact macro event.
    /// </summary>
    public class EconomicCalendarEvent
    {
        public EconomicCalendarEventType Type { get; set; }

        public string Title { get; set; }

        public string Source { get; set; }

        public DateTime OccursAtUtc { get; set; }

        public decimal ImpactScore { get; set; }
    }
}
