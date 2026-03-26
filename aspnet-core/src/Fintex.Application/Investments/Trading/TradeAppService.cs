using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Events.Bus;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Events;
using Fintex.Investments.Trading.Dto;
using System;
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
        private readonly IEventBus _eventBus;

        public TradeAppService(
            ITradeRepository tradeRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IEventBus eventBus)
        {
            _tradeRepository = tradeRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _eventBus = eventBus;
        }

        public async Task<ListResultDto<TradeDto>> GetMyTradesAsync()
        {
            var userId = AbpSession.GetUserId();
            var trades = await _tradeRepository.GetUserTradesAsync(userId);
            return new ListResultDto<TradeDto>(ObjectMapper.Map<System.Collections.Generic.List<TradeDto>>(trades));
        }

        public async Task<TradeDto> GetAsync(EntityDto<long> input)
        {
            var trade = await _tradeRepository.FirstOrDefaultAsync(input.Id);
            if (trade == null || trade.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException("Trade not found.");
            }

            return ObjectMapper.Map<TradeDto>(trade);
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

            return ObjectMapper.Map<TradeDto>(trade);
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

            return ObjectMapper.Map<TradeDto>(trade);
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
