using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Events;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        public async Task<PaperTradeExecutionResultDto> PlaceMarketOrderAsync(PlacePaperOrderInput input)
        {
            var userId = AbpSession.GetUserId();
            await _academyProgressService.EnsureTradeAcademyAccessAsync(userId, AbpSession.TenantId);
            var account = await GetMyAccountOrThrowAsync(userId);
            var marketContext = await GetMarketContextAsync(input.Symbol, input.Provider);
            var assessment = BuildTradeAssessment(
                account,
                marketContext,
                input.Direction,
                input.Quantity,
                input.StopLoss,
                input.TakeProfit);

            if (assessment.ShouldBlock)
            {
                return new PaperTradeExecutionResultDto
                {
                    WasExecuted = false,
                    Assessment = assessment
                };
            }

            var occurredAt = DateTime.UtcNow;
            var fillPrice = marketContext.LatestPoint.Price;
            var effectiveStopLoss = input.StopLoss ?? assessment.SuggestedStopLoss;
            var effectiveTakeProfit = input.TakeProfit ?? assessment.SuggestedTakeProfit;
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
                effectiveStopLoss,
                effectiveTakeProfit,
                input.Notes,
                occurredAt);

            await _paperOrderRepository.InsertAsync(order);
            await CurrentUnitOfWork.SaveChangesAsync();

            var position = new PaperPosition(
                AbpSession.TenantId,
                userId,
                account.Id,
                input.Symbol,
                input.AssetClass,
                input.Provider,
                input.Direction,
                input.Quantity,
                fillPrice,
                effectiveStopLoss,
                effectiveTakeProfit,
                occurredAt);

            await _paperPositionRepository.InsertAsync(position);
            await CurrentUnitOfWork.SaveChangesAsync();

            var positionId = position.Id;
            var realizedProfitLoss = 0m;

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
            await _eventBus.TriggerAsync(new TradeExecutedEventData
            {
                TenantId = AbpSession.TenantId,
                TradeId = order.Id,
                UserId = userId,
                PositionId = positionId,
                Symbol = input.Symbol,
                Provider = input.Provider,
                Direction = input.Direction,
                Quantity = input.Quantity,
                ExecutionPrice = fillPrice,
                Source = "Paper academy",
                Status = position.Status == PaperPositionStatus.Open ? TradeStatus.Open : TradeStatus.Closed,
                RealizedProfitLoss = realizedProfitLoss,
                OccurredAt = occurredAt
            });

            return new PaperTradeExecutionResultDto
            {
                WasExecuted = true,
                Assessment = assessment,
                Order = ObjectMapper.Map<PaperOrderDto>(order)
            };
        }

        public async Task<PaperOrderDto> ClosePositionAsync(ClosePaperPositionInput input)
        {
            var userId = AbpSession.GetUserId();
            await _academyProgressService.EnsureTradeAcademyAccessAsync(userId, AbpSession.TenantId);
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

            var originalQuantity = position.Quantity;
            var quantity = input.Quantity ?? position.Quantity;
            if (quantity <= 0m || quantity > position.Quantity)
            {
                throw new UserFriendlyException("Close quantity must be greater than zero and not exceed the open position size.");
            }

            var occurredAt = DateTime.UtcNow;
            var fillPrice = input.ExitPrice ?? await ResolveLatestPriceAsync(position.Symbol, position.Provider);
            var closingDirection = position.Direction == TradeDirection.Buy ? TradeDirection.Sell : TradeDirection.Buy;

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
            await _eventBus.TriggerAsync(new TradeExecutedEventData
            {
                TenantId = AbpSession.TenantId,
                TradeId = order.Id,
                UserId = userId,
                PositionId = position.Id,
                Symbol = position.Symbol,
                Provider = position.Provider,
                Direction = closingDirection,
                Quantity = quantity,
                ExecutionPrice = fillPrice,
                Source = "Paper academy",
                Status = quantity >= originalQuantity ? TradeStatus.Closed : TradeStatus.Open,
                RealizedProfitLoss = realizedProfitLoss,
                OccurredAt = occurredAt
            });

            return ObjectMapper.Map<PaperOrderDto>(order);
        }
    }
}
