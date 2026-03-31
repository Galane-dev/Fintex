using Abp.Application.Services.Dto;
using System.Collections.Generic;

namespace Fintex.Investments.EconomicCalendar.Dto
{
    /// <summary>
    /// Wraps the current BTC/USD macro-event risk view for the dashboard calendar modal.
    /// </summary>
    public class MacroCalendarInsightDto : EntityDto<int>
    {
        public string Summary { get; set; }

        public decimal RiskScore { get; set; }

        public string NextEventAtUtc { get; set; }

        public List<MacroCalendarEventDto> UpcomingEvents { get; set; } = new List<MacroCalendarEventDto>();
    }
}
