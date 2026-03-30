using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.EconomicCalendar
{
    /// <summary>
    /// Loads upcoming macro events and converts them into recommendation risk context.
    /// </summary>
    public interface IEconomicCalendarService
    {
        Task<EconomicCalendarInsight> GetBitcoinUsdRiskInsightAsync(CancellationToken cancellationToken);
    }
}
