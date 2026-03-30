using Abp.Application.Services;
using Fintex.Investments.EconomicCalendar.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.EconomicCalendar
{
    /// <summary>
    /// Exposes the macro-event risk layer used by dashboard recommendations.
    /// </summary>
    public interface IEconomicCalendarAppService : IApplicationService
    {
        Task<MacroCalendarInsightDto> GetBitcoinUsdRiskInsightAsync();
    }
}
