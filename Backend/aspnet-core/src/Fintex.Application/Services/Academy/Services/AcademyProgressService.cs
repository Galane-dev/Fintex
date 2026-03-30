using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Dependency;
using Abp.UI;
using Fintex.Investments;
using System;
using System.Threading.Tasks;

namespace Fintex.Investments.Academy
{
    /// <summary>
    /// Centralizes academy status, graduation checks, and access gates.
    /// </summary>
    public class AcademyProgressService : IAcademyProgressService, ITransientDependency
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IPaperTradingAccountRepository _paperTradingAccountRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AcademyProgressService(
            IUserProfileRepository userProfileRepository,
            IPaperTradingAccountRepository paperTradingAccountRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _userProfileRepository = userProfileRepository;
            _paperTradingAccountRepository = paperTradingAccountRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<AcademyProgressState> GetStatusAsync(long userId, int? tenantId)
        {
            var profile = await GetOrCreateProfileAsync(userId, tenantId);
            var paperAccount = await GetPaperAccountAsync(userId);
            var paperGrowthPercent = CalculateGrowthPercent(paperAccount);
            var updated = profile.SyncAcademyGraduation(
                paperGrowthPercent >= AcademyContent.GraduationGrowthTargetPercent,
                DateTime.UtcNow);

            if (updated)
            {
                await _userProfileRepository.UpdateAsync(profile);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            }

            return new AcademyProgressState
            {
                Profile = profile,
                PaperAccount = paperAccount,
                PaperGrowthPercent = paperGrowthPercent,
                GrowthTargetPercent = AcademyContent.GraduationGrowthTargetPercent
            };
        }

        public async Task EnsureTradeAcademyAccessAsync(long userId, int? tenantId)
        {
            var status = await GetStatusAsync(userId, tenantId);
            if (status.Profile.IntroQuizPassedAt.HasValue)
            {
                return;
            }

            throw new UserFriendlyException("Complete the Fintex intro academy quiz with at least 90% before entering trade academy.");
        }

        public async Task EnsureExternalBrokerAccessAsync(long userId, int? tenantId)
        {
            var status = await GetStatusAsync(userId, tenantId);
            if (status.Profile.AcademyStage == AcademyStage.Graduated)
            {
                return;
            }

            throw new UserFriendlyException("External brokers unlock only after you pass the intro quiz and grow your academy paper account by 75%.");
        }

        private async Task<UserProfile> GetOrCreateProfileAsync(long userId, int? tenantId)
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile != null)
            {
                return profile;
            }

            profile = new UserProfile(tenantId, userId, "USD");
            await _userProfileRepository.InsertAsync(profile);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            return profile;
        }

        private async Task<PaperTradingAccount> GetPaperAccountAsync(long userId)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                return await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            }
        }

        private static decimal CalculateGrowthPercent(PaperTradingAccount account)
        {
            if (account == null || account.StartingBalance <= 0m)
            {
                return 0m;
            }

            var growth = ((account.Equity - account.StartingBalance) / account.StartingBalance) * 100m;
            return decimal.Round(growth, 2, MidpointRounding.AwayFromZero);
        }
    }
}
