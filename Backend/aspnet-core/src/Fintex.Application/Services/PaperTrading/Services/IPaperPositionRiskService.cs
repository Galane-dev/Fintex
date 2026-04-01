using Fintex.Investments.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    /// <summary>
    /// Evaluates live market updates against open paper positions and closes any risk exits that are hit.
    /// </summary>
    public interface IPaperPositionRiskService
    {
        Task EvaluateAsync(MarketDataUpdatedEventData eventData, CancellationToken cancellationToken);
    }
}
