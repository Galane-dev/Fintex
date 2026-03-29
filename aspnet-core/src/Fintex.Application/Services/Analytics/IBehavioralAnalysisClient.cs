using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Calls an external AI provider for user behavior analysis.
    /// </summary>
    public interface IBehavioralAnalysisClient
    {
        Task<UserBehaviorInsight> AnalyzeAsync(UserProfile profile, IReadOnlyList<BehaviorTradeActivity> recentTrades, CancellationToken cancellationToken);
    }
}
