using Fintex.Investments.Trading.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Trading
{
    /// <summary>
    /// Builds coaching notes for closed trades using stored execution context and recent behavior.
    /// </summary>
    public interface ITradeReviewService
    {
        Task<IReadOnlyDictionary<long, ClosedTradeReviewDto>> BuildClosedTradeReviewsAsync(
            IReadOnlyList<Trade> closedTrades,
            IReadOnlyList<Trade> recentTrades,
            IReadOnlyDictionary<long, TradeExecutionContext> executionContexts,
            UserProfile profile,
            CancellationToken cancellationToken);
    }
}
