using Abp.Authorization;
using Fintex.Investments.EconomicCalendar.Dto;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.EconomicCalendar
{
    /// <summary>
    /// Returns the current macro-event risk view for BTC/USD users in the dashboard.
    /// </summary>
    [AbpAuthorize]
    public class EconomicCalendarAppService : FintexAppServiceBase, IEconomicCalendarAppService
    {
        private readonly IEconomicCalendarService _economicCalendarService;

        public EconomicCalendarAppService(IEconomicCalendarService economicCalendarService)
        {
            _economicCalendarService = economicCalendarService;
        }

        public async Task<MacroCalendarInsightDto> GetBitcoinUsdRiskInsightAsync()
        {
            var insight = await _economicCalendarService.GetBitcoinUsdRiskInsightAsync(CancellationToken.None);

            return new MacroCalendarInsightDto
            {
                Id = 1,
                Summary = insight.Summary,
                RiskScore = insight.RiskScore,
                NextEventAtUtc = insight.NextEventAtUtc?.ToString("O"),
                UpcomingEvents = insight.UpcomingEvents
                    .Select(item => new MacroCalendarEventDto
                    {
                        Title = item.Title,
                        Source = item.Source,
                        OccursAtUtc = item.OccursAtUtc.ToString("O"),
                        ImpactScore = item.ImpactScore
                    })
                    .ToList()
            };
        }
    }
}
