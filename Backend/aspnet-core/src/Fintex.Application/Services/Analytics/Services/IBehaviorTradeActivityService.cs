using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Loads recent user trade activity from both live and paper sources for behavioral analysis.
    /// </summary>
    public interface IBehaviorTradeActivityService
    {
        Task<IReadOnlyList<BehaviorTradeActivity>> GetRecentActivityAsync(long userId, int take, CancellationToken cancellationToken);
    }
}
