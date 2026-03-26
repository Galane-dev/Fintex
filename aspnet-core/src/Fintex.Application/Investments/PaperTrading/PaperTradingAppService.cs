using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    /// <summary>
    /// Internal paper trading execution service built on top of live market prices.
    /// </summary>
    [AbpAuthorize]
    public class PaperTradingAppService : FintexAppServiceBase, IPaperTradingAppService
    {
        private readonly IPaperTradingAccountRepository _paperTradingAccountRepository;
        private readonly IPaperOrderRepository _paperOrderRepository;
        private readonly IPaperPositionRepository _paperPositionRepository;
        private readonly IPaperTradeFillRepository _paperTradeFillRepository;
        private readonly IMarketDataPointRepository _marketDataPointRepository;

        public PaperTradingAppService(
            IPaperTradingAccountRepository paperTradingAccountRepository,
            IPaperOrderRepository paperOrderRepository,
            IPaperPositionRepository paperPositionRepository,
            IPaperTradeFillRepository paperTradeFillRepository,
            IMarketDataPointRepository marketDataPointRepository)
        {
            _paperTradingAccountRepository = paperTradingAccountRepository;
            _paperOrderRepository = paperOrderRepository;
            _paperPositionRepository = paperPositionRepository;
            _paperTradeFillRepository = paperTradeFillRepository;
            _marketDataPointRepository = marketDataPointRepository;
        }

        public async Task<PaperTradingAccountDto> CreateMyAccountAsync(CreatePaperTradingAccountInput input)
        {
            var userId = AbpSession.GetUserId();
            var existing = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (existing != null)
            {
                throw new UserFriendlyException("You already have an active paper trading account.");
            }

            var account = new PaperTradingAccount(
                AbpSession.TenantId,
                userId,
                input.Name,
                input.BaseCurrency,
                input.StartingBalance);

            await _paperTradingAccountRepository.InsertAsync(account);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PaperTradingAccountDto>(account);
        }

        public async Task<PaperTradingSnapshotDto> GetMySnapshotAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);

            await MarkAccountToMarketAsync(account, positions, DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();

            var orders = await _paperOrderRepository.GetUserOrdersAsync(userId, account.Id);
            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);

            return new PaperTradingSnapshotDto
            {
                Account = ObjectMapper.Map<PaperTradingAccountDto>(account),
                Positions = ObjectMapper.Map<List<PaperPositionDto>>(positions),
                RecentOrders = ObjectMapper.Map<List<PaperOrderDto>>(orders.Take(20).ToList()),
                RecentFills = ObjectMapper.Map<List<PaperTradeFillDto>>(fills.Take(20).ToList())
            };
        }

        public async Task<ListResultDto<PaperOrderDto>> GetMyOrdersAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var orders = await _paperOrderRepository.GetUserOrdersAsync(userId, account.Id);
            return new ListResultDto<PaperOrderDto>(ObjectMapper.Map<List<PaperOrderDto>>(orders));
        }

        public async Task<ListResultDto<PaperPositionDto>> GetMyPositionsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);

            await MarkAccountToMarketAsync(account, positions, DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new ListResultDto<PaperPositionDto>(ObjectMapper.Map<List<PaperPositionDto>>(positions));
        }

        public async Task<ListResultDto<PaperTradeFillDto>> GetMyFillsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);
            return new ListResultDto<PaperTradeFillDto>(ObjectMapper.Map<List<PaperTradeFillDto>>(fills));
        }

        public async Task<PaperOrderDto> PlaceMarketOrderAsync(PlacePaperOrderInput input)
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var occurredAt = DateTime.UtcNow;
            var fillPrice = await ResolveLatestPriceAsync(input.Symbol, input.Provider);

            var order = new PaperOrder(
                AbpSession.TenantId,
                userId,
                account.Id,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                PaperOrderType.Market,
                input.Quantity,
                fillPrice,
                input.StopLoss,
                input.TakeProfit,
                input.Notes,
                occurredAt);

            await _paperOrderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            var position = await _paperPositionRepository.GetOpenBySymbolAsync(account.Id, input.Symbol, input.Provider);
            var realizedProfitLoss = 0m;
            long? positionId;

            if (position == null)
            {
                position = new PaperPosition(
                    AbpSession.TenantId,
                    userId,
                    account.Id,
                    input.Symbol,
                    input.AssetClass,
                    input.Provider,
                    input.Direction,
                    input.Quantity,
                    fillPrice,
                    input.StopLoss,
                    input.TakeProfit,
                    occurredAt);

                await _paperPositionRepository.InsertAsync(position);
                await CurrentUnitOfWork.SaveChangesAsync();
                positionId = position.Id;
            }
            else if (position.Direction == input.Direction)
            {
                position.Add(input.Quantity, fillPrice, occurredAt);
                positionId = position.Id;
            }
            else
            {
                var closingQuantity = decimal.Min(position.Quantity, input.Quantity);
                realizedProfitLoss = position.Reduce(closingQuantity, fillPrice, occurredAt);
                account.ApplyRealizedProfitLoss(realizedProfitLoss, occurredAt);

                var remainingQuantity = input.Quantity - closingQuantity;
                if (remainingQuantity > 0m)
                {
                    position = new PaperPosition(
                        AbpSession.TenantId,
                        userId,
                        account.Id,
                        input.Symbol,
                        input.AssetClass,
                        input.Provider,
                        input.Direction,
                        remainingQuantity,
                        fillPrice,
                        input.StopLoss,
                        input.TakeProfit,
                        occurredAt);

                    await _paperPositionRepository.InsertAsync(position);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                positionId = position.Id;
            }

            order.MarkFilled(fillPrice, occurredAt, positionId);
            await _paperTradeFillRepository.InsertAsync(new PaperTradeFill(
                AbpSession.TenantId,
                userId,
                account.Id,
                order.Id,
                positionId,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                input.Quantity,
                fillPrice,
                realizedProfitLoss,
                occurredAt));

            var openPositions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            await MarkAccountToMarketAsync(account, openPositions, occurredAt);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PaperOrderDto>(order);
        }

        public async Task<PaperOrderDto> ClosePositionAsync(ClosePaperPositionInput input)
        {
            var userId = AbpSession.GetUserId();
            var account = await GetMyAccountOrThrowAsync(userId);
            var position = await _paperPositionRepository.FirstOrDefaultAsync(input.PositionId);

            if (position == null || position.UserId != userId || position.AccountId != account.Id)
            {
                throw new UserFriendlyException("Position not found.");
            }

            if (position.Status != PaperPositionStatus.Open)
            {
                throw new UserFriendlyException("Only open positions can be closed.");
            }

            var quantity = input.Quantity ?? position.Quantity;
            if (quantity <= 0m || quantity > position.Quantity)
            {
                throw new UserFriendlyException("Close quantity must be greater than zero and not exceed the open position size.");
            }

            var occurredAt = DateTime.UtcNow;
            var fillPrice = input.ExitPrice ?? await ResolveLatestPriceAsync(position.Symbol, position.Provider);
            var closingDirection = position.Direction == TradeDirection.Buy
                ? TradeDirection.Sell
                : TradeDirection.Buy;

            var order = new PaperOrder(
                AbpSession.TenantId,
                userId,
                account.Id,
                position.Symbol,
                position.AssetClass,
                position.Provider,
                closingDirection,
                PaperOrderType.Market,
                quantity,
                fillPrice,
                position.StopLoss,
                position.TakeProfit,
                "Position close",
                occurredAt);

            await _paperOrderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            var realizedProfitLoss = position.Reduce(quantity, fillPrice, occurredAt);
            account.ApplyRealizedProfitLoss(realizedProfitLoss, occurredAt);

            order.MarkFilled(fillPrice, occurredAt, position.Id);
            await _paperTradeFillRepository.InsertAsync(new PaperTradeFill(
                AbpSession.TenantId,
                userId,
                account.Id,
                order.Id,
                position.Id,
                position.Symbol,
                position.AssetClass,
                position.Provider,
                closingDirection,
                quantity,
                fillPrice,
                realizedProfitLoss,
                occurredAt));

            var openPositions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            await MarkAccountToMarketAsync(account, openPositions, occurredAt);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PaperOrderDto>(order);
        }

        private async Task<PaperTradingAccount> GetMyAccountOrThrowAsync(long userId)
        {
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                throw new UserFriendlyException("Create a paper trading account before placing simulated trades.");
            }

            return account;
        }

        private async Task MarkAccountToMarketAsync(
            PaperTradingAccount account,
            List<PaperPosition> positions,
            DateTime occurredAt)
        {
            decimal totalUnrealized = 0m;

            foreach (var position in positions)
            {
                var marketPrice = await ResolveLatestPriceAsync(position.Symbol, position.Provider);
                position.RefreshMarketPrice(marketPrice, occurredAt);
                totalUnrealized += position.UnrealizedProfitLoss;
            }

            account.ApplyMarkToMarket(totalUnrealized, occurredAt);
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
