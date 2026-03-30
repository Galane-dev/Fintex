using Abp.Dependency;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Builds a unified behavioral activity feed from live trades and paper fills.
    /// </summary>
    public class BehaviorTradeActivityService : IBehaviorTradeActivityService, ITransientDependency
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IPaperTradeFillRepository _paperTradeFillRepository;

        public BehaviorTradeActivityService(
            ITradeRepository tradeRepository,
            IPaperTradeFillRepository paperTradeFillRepository)
        {
            _tradeRepository = tradeRepository;
            _paperTradeFillRepository = paperTradeFillRepository;
        }

        public async Task<IReadOnlyList<BehaviorTradeActivity>> GetRecentActivityAsync(long userId, int take, CancellationToken cancellationToken)
        {
            var normalizedTake = take <= 0 ? 20 : take;

            var trades = await _tradeRepository.GetAll()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.ClosedAt ?? item.ExecutedAt)
                .Take(normalizedTake)
                .ToListAsync(cancellationToken);

            var paperFills = await _paperTradeFillRepository.GetAll()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.ExecutedAt)
                .Take(normalizedTake)
                .ToListAsync(cancellationToken);

            return trades
                .Select(MapTrade)
                .Concat(paperFills.Select(MapPaperFill))
                .OrderByDescending(item => item.OccurredAt)
                .Take(normalizedTake)
                .ToList();
        }

        private static BehaviorTradeActivity MapTrade(Trade trade)
        {
            return new BehaviorTradeActivity
            {
                Source = "Live trade",
                Symbol = trade.Symbol,
                AssetClass = trade.AssetClass,
                Direction = trade.Direction,
                Quantity = trade.Quantity,
                EntryPrice = trade.EntryPrice,
                ExitPrice = trade.ExitPrice,
                RealizedProfitLoss = trade.RealizedProfitLoss,
                Status = trade.Status.ToString(),
                OccurredAt = trade.ClosedAt ?? trade.ExecutedAt
            };
        }

        private static BehaviorTradeActivity MapPaperFill(PaperTradeFill fill)
        {
            return new BehaviorTradeActivity
            {
                Source = "Paper academy",
                Symbol = fill.Symbol,
                AssetClass = fill.AssetClass,
                Direction = fill.Direction,
                Quantity = fill.Quantity,
                EntryPrice = fill.Price,
                ExitPrice = fill.Price,
                RealizedProfitLoss = fill.RealizedProfitLoss,
                Status = "Filled",
                OccurredAt = fill.ExecutedAt
            };
        }
    }
}
