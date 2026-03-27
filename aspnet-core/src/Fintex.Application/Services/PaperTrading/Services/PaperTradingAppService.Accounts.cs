using Abp.Application.Services.Dto;
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
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new PaperTradingSnapshotDto();
            }

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
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
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
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperPositionDto>(new List<PaperPositionDto>());
            }

            var positions = await _paperPositionRepository.GetOpenPositionsAsync(account.Id);
            await MarkAccountToMarketAsync(account, positions, DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new ListResultDto<PaperPositionDto>(ObjectMapper.Map<List<PaperPositionDto>>(positions));
        }

        public async Task<ListResultDto<PaperTradeFillDto>> GetMyFillsAsync()
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            if (account == null)
            {
                return new ListResultDto<PaperTradeFillDto>(new List<PaperTradeFillDto>());
            }

            var fills = await _paperTradeFillRepository.GetAccountFillsAsync(account.Id);
            return new ListResultDto<PaperTradeFillDto>(ObjectMapper.Map<List<PaperTradeFillDto>>(fills));
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
    }
}
