using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        public async Task<PaperTradingAccountDto> CreateMyAccountAsync(CreatePaperTradingAccountInput input)
        {
            var userId = AbpSession.GetUserId();
            await _academyProgressService.EnsureTradeAcademyAccessAsync(userId, AbpSession.TenantId);
            var existing = await GetActiveAccountAsync(userId);
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
            var account = await GetActiveAccountAsync(userId);
            if (account == null)
            {
                return new PaperTradingSnapshotDto();
            }

            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            var markedToMarket = await BuildMarkedToMarketViewAsync(account, positions, DateTime.UtcNow);

            var orders = await _paperOrderRepository.GetUserOrdersAsync(userId, account.Id);
            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);

            return new PaperTradingSnapshotDto
            {
                Account = markedToMarket.Account,
                Positions = markedToMarket.Positions,
                RecentOrders = ObjectMapper.Map<List<PaperOrderDto>>(orders),
                RecentFills = ObjectMapper.Map<List<PaperTradeFillDto>>(fills)
            };
        }

        public async Task<ListResultDto<PaperOrderDto>> GetMyOrdersAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetActiveAccountAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperOrderDto>(new List<PaperOrderDto>());
            }

            var orders = await _paperOrderRepository.GetUserOrdersAsync(userId, account.Id);
            return new ListResultDto<PaperOrderDto>(ObjectMapper.Map<List<PaperOrderDto>>(orders));
        }

        public async Task<ListResultDto<PaperPositionDto>> GetMyPositionsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetActiveAccountAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperPositionDto>(new List<PaperPositionDto>());
            }

            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            var markedToMarket = await BuildMarkedToMarketViewAsync(account, positions, DateTime.UtcNow);

            return new ListResultDto<PaperPositionDto>(markedToMarket.Positions);
        }

        public async Task<ListResultDto<PaperTradeFillDto>> GetMyFillsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await GetActiveAccountAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperTradeFillDto>(new List<PaperTradeFillDto>());
            }

            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);
            return new ListResultDto<PaperTradeFillDto>(ObjectMapper.Map<List<PaperTradeFillDto>>(fills));
        }

        private async Task<PaperTradingAccount> GetMyAccountOrThrowAsync(long userId)
        {
            var account = await GetActiveAccountAsync(userId);
            if (account == null)
            {
                throw new UserFriendlyException("Create a paper trading account before placing simulated trades.");
            }

            return account;
        }

        private async Task<PaperTradingAccount> GetActiveAccountAsync(long userId)
        {
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account != null)
            {
                return account;
            }

            // Background evaluators can run from a host-scoped market-data pipeline.
            // Fall back to a tenant-filter-disabled lookup so existing user accounts stay visible.
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                return await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            }
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

        private async Task<(PaperTradingAccountDto Account, List<PaperPositionDto> Positions)> BuildMarkedToMarketViewAsync(
            PaperTradingAccount account,
            List<PaperPosition> positions,
            DateTime occurredAt)
        {
            decimal totalUnrealized = 0m;
            var positionDtos = new List<PaperPositionDto>(positions.Count);

            foreach (var position in positions)
            {
                var marketPrice = await ResolveLatestDisplayPriceAsync(position);
                var unrealized = Trade.CalculateProfitLoss(
                    position.AverageEntryPrice,
                    marketPrice,
                    position.Quantity,
                    position.Direction);

                totalUnrealized += unrealized;
                positionDtos.Add(new PaperPositionDto
                {
                    Id = position.Id,
                    AccountId = position.AccountId,
                    Symbol = position.Symbol,
                    AssetClass = position.AssetClass,
                    Provider = position.Provider,
                    Direction = position.Direction,
                    Status = position.Status,
                    Quantity = position.Quantity,
                    AverageEntryPrice = position.AverageEntryPrice,
                    CurrentMarketPrice = marketPrice,
                    RealizedProfitLoss = position.RealizedProfitLoss,
                    UnrealizedProfitLoss = unrealized,
                    StopLoss = position.StopLoss,
                    TakeProfit = position.TakeProfit,
                    OpenedAt = position.OpenedAt,
                    LastUpdatedAt = occurredAt,
                    ClosedAt = position.ClosedAt
                });
            }

            var accountDto = ObjectMapper.Map<PaperTradingAccountDto>(account);
            accountDto.UnrealizedProfitLoss = totalUnrealized;
            accountDto.Equity = account.CashBalance + totalUnrealized;
            accountDto.LastMarkedToMarketAt = occurredAt;

            return (accountDto, positionDtos);
        }

        private async Task<decimal> ResolveLatestDisplayPriceAsync(PaperPosition position)
        {
            try
            {
                return await ResolveLatestPriceAsync(position.Symbol, position.Provider);
            }
            catch (UserFriendlyException)
            {
                return position.CurrentMarketPrice > 0m
                    ? position.CurrentMarketPrice
                    : position.AverageEntryPrice;
            }
        }
    }
}
