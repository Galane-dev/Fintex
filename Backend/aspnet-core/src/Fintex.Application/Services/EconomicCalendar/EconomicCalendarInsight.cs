using System;
using System.Collections.Generic;

namespace Fintex.Investments.EconomicCalendar
{
    /// <summary>
    /// Risk summary built from upcoming macro events.
    /// </summary>
    public class EconomicCalendarInsight
    {
        public string Summary { get; set; }

        public decimal RiskScore { get; set; }

        public DateTime? NextEventAtUtc { get; set; }

        public List<EconomicCalendarEvent> UpcomingEvents { get; set; } = new List<EconomicCalendarEvent>();
    }
}
