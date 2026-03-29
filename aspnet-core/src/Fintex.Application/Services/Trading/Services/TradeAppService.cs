using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Events;
using Fintex.Investments.Trading.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Trading
{
    /// <summary>
    /// Manages user trade lifecycles using live market prices as defaults.
    /// </summary>
    [AbpAuthorize]
    public class TradeAppService : FintexAppServiceBase, ITradeAppService
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IRepository<TradeExecutionContext, long> _tradeExecutionContextRepository;
        private readonly IRepository<UserProfile, long> _userProfileRepository;
        private readonly ITradeReviewService _tradeReviewService;
        private readonly IEventBus _eventBus;

        public TradeAppService(
            ITradeRepository tradeRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IRepository<TradeExecutionContext, long> tradeExecutionContextRepository,
            IRepository<UserProfile, long> userProfileRepository,
            ITradeReviewService tradeReviewService,
            IEventBus eventBus)
        {
            _tradeRepository = tradeRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _tradeExecutionContextRepository = tradeExecutionContextRepository;
            _userProfileRepository = userProfileRepository;
            _tradeReviewService = tradeReviewService;
            _eventBus = eventBus;
        }

        public async Task<ListResultDto<TradeDto>> GetMyTradesAsync()
        {
            var userId = AbpSession.GetUserId();
            var trades = await _tradeRepository.GetUserTradesAsync(userId);
            var contexts = await GetExecutionContextsAsync(trades.Select(x => x.Id));
            var reviews = await BuildClosedTradeReviewsAsync(userId, trades, contexts, CancellationToken.None);
            return new ListResultDto<TradeDto>(trades.Select(trade => MapTradeDto(trade, contexts, reviews)).ToList());
        }

        public async Task<TradeDto> GetAsync(EntityDto<long> input)
        {
            var trade = await _tradeRepository.FirstOrDefaultAsync(input.Id);
            if (trade == null || trade.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException("Trade not found.");
            }

            var recentTrades = await _tradeRepository.GetUserTradesAsync(trade.UserId);
            var contexts = await GetExecutionContextsAsync(recentTrades.Select(item => item.Id));
            var reviews = await BuildClosedTradeReviewsAsync(trade.UserId, recentTrades, contexts, CancellationToken.None);
            return MapTradeDto(trade, contexts, reviews);
        }

        public async Task<TradeDto> CreateAsync(CreateTradeInput input)
        {
            var user = await GetCurrentUserAsync();
            var entryPrice = input.EntryPrice ?? await ResolveLatestPriceAsync(input.Symbol, input.Provider);
            var trade = new Trade(
                AbpSession.TenantId,
                user.Id,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                input.Quantity,
                entryPrice,
                input.ExecutedAt ?? DateTime.UtcNow,
                input.StopLoss,
                input.TakeProfit,
                input.ExternalOrderId,
                input.Notes);

            trade.RefreshMarketPrice(entryPrice);
            await _tradeRepository.InsertAsync(trade);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _eventBus.TriggerAsync(new TradeExecutedEventData
            {
                TenantId = trade.TenantId,
                TradeId = trade.Id,
                UserId = trade.UserId,
                Symbol = trade.Symbol,
                Status = trade.Status,
                RealizedProfitLoss = trade.RealizedProfitLoss,
                OccurredAt = trade.ExecutedAt
            });

            var contexts = await GetExecutionContextsAsync(new[] { trade.Id });
            return MapTradeDto(trade, contexts, new Dictionary<long, ClosedTradeReviewDto>());
        }

        public async Task<TradeDto> CloseAsync(CloseTradeInput input)
        {
            var trade = await _tradeRepository.FirstOrDefaultAsync(input.TradeId);
            if (trade == null || trade.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException("Trade not found.");
            }

            var exitPrice = input.ExitPrice ?? await ResolveLatestPriceAsync(trade.Symbol, trade.Provider);
            trade.Close(exitPrice, input.ClosedAt ?? DateTime.UtcNow);
            await _tradeRepository.UpdateAsync(trade);

            await _eventBus.TriggerAsync(new TradeExecutedEventData
            {
                TenantId = trade.TenantId,
                TradeId = trade.Id,
                UserId = trade.UserId,
                Symbol = trade.Symbol,
                Status = trade.Status,
                RealizedProfitLoss = trade.RealizedProfitLoss,
                OccurredAt = trade.ClosedAt ?? DateTime.UtcNow
            });

            var recentTrades = await _tradeRepository.GetUserTradesAsync(trade.UserId);
            var contexts = await GetExecutionContextsAsync(recentTrades.Select(item => item.Id));
            var reviews = await BuildClosedTradeReviewsAsync(trade.UserId, recentTrades, contexts, CancellationToken.None);
            return MapTradeDto(trade, contexts, reviews);
        }

        private async Task<Dictionary<long, TradeExecutionContext>> GetExecutionContextsAsync(IEnumerable<long> tradeIds)
        {
            var ids = tradeIds?.Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0)
            {
                return new Dictionary<long, TradeExecutionContext>();
            }

            var contexts = await _tradeExecutionContextRepository.GetAllListAsync(x => ids.Contains(x.TradeId));
            return contexts
                .GroupBy(x => x.TradeId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(item => item.CreationTime).First());
        }

        private async Task<IReadOnlyDictionary<long, ClosedTradeReviewDto>> BuildClosedTradeReviewsAsync(
            long userId,
            IReadOnlyList<Trade> recentTrades,
            IReadOnlyDictionary<long, TradeExecutionContext> contexts,
            CancellationToken cancellationToken)
        {
            var closedTrades = recentTrades
                .Where(trade => trade.Status == TradeStatus.Closed)
                .OrderByDescending(trade => trade.ClosedAt ?? trade.ExecutedAt)
                .ToList();

            if (closedTrades.Count == 0)
            {
                return new Dictionary<long, ClosedTradeReviewDto>();
            }

            var profile = await _userProfileRepository.FirstOrDefaultAsync(item => item.UserId == userId);
            return await _tradeReviewService.BuildClosedTradeReviewsAsync(closedTrades, recentTrades, contexts, profile, cancellationToken);
        }

        private TradeDto MapTradeDto(
            Trade trade,
            IReadOnlyDictionary<long, TradeExecutionContext> contexts,
            IReadOnlyDictionary<long, ClosedTradeReviewDto> reviews)
        {
            var dto = ObjectMapper.Map<TradeDto>(trade);
            if (contexts.TryGetValue(trade.Id, out var context) && context != null)
            {
                dto.StopLoss ??= context.StopLoss;
                dto.TakeProfit ??= context.TakeProfit;
            }

            if (trade.Status == TradeStatus.Closed && reviews.TryGetValue(trade.Id, out var review))
            {
                dto.ClosedTradeReview = review;
            }

            return dto;
        }

        private async Task<decimal> ResolveLatestPriceAsync(string symbol, MarketDataProvider provider)
        {
            var latestPoint = await _marketDataPointRepository.GetLatestAsync(symbol, provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException("No live market price is available yet for the requested symbol.");
            }

            return latestPoint.Price;
        }
    }
}
