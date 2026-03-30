namespace Fintex.Investments.EconomicCalendar.Dto
{
    /// <summary>
    /// Describes a single upcoming macro event shown in the dashboard calendar modal.
    /// </summary>
    public class MacroCalendarEventDto
    {
        public string Title { get; set; }

        public string Source { get; set; }

        public string OccursAtUtc { get; set; }

        public decimal ImpactScore { get; set; }
    }
}
