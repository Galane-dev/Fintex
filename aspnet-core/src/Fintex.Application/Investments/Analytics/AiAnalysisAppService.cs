using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Profiles.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Exposes stored trade analytics and refreshes behavioral AI summaries on demand.
    /// </summary>
    [AbpAuthorize]
    public class AiAnalysisAppService : FintexAppServiceBase, IAiAnalysisAppService
    {
        private readonly IRepository<TradeAnalysisSnapshot, long> _snapshotRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IBehavioralAnalysisClient _behavioralAnalysisClient;

        public AiAnalysisAppService(
            IRepository<TradeAnalysisSnapshot, long> snapshotRepository,
            ITradeRepository tradeRepository,
            IUserProfileRepository userProfileRepository,
            IBehavioralAnalysisClient behavioralAnalysisClient)
        {
            _snapshotRepository = snapshotRepository;
            _tradeRepository = tradeRepository;
            _userProfileRepository = userProfileRepository;
            _behavioralAnalysisClient = behavioralAnalysisClient;
        }

        public async Task<ListResultDto<Dto.TradeAnalysisSnapshotDto>> GetTradeSnapshotsAsync(EntityDto<long> input)
        {
            var trade = await _tradeRepository.FirstOrDefaultAsync(input.Id);
            if (trade == null || trade.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException("Trade not found.");
            }

            var snapshots = await _snapshotRepository.GetAll()
                .Where(x => x.TradeId == trade.Id)
                .OrderByDescending(x => x.GeneratedAt)
                .Take(50)
                .ToListAsync();

            return new ListResultDto<Dto.TradeAnalysisSnapshotDto>(
                ObjectMapper.Map<System.Collections.Generic.List<Dto.TradeAnalysisSnapshotDto>>(snapshots));
        }

        public async Task<UserProfileDto> RefreshMyBehavioralProfileAsync()
        {
            var userId = AbpSession.GetUserId();
            var profile = await _userProfileRepository.GetByUserIdAsync(userId) ?? new UserProfile(AbpSession.TenantId, userId, "USD");
            if (profile.Id == 0)
            {
                await _userProfileRepository.InsertAsync(profile);
            }

            var recentTrades = await _tradeRepository.GetAll()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreationTime)
                .Take(20)
                .ToListAsync();

            var insight = await _behavioralAnalysisClient.AnalyzeAsync(profile, recentTrades, CancellationToken.None);
            if (insight.WasGenerated)
            {
                profile.ApplyBehavioralInsight(insight.RiskScore, insight.Summary, insight.Provider, insight.Model, DateTime.UtcNow);
                await _userProfileRepository.UpdateAsync(profile);
            }

            return ObjectMapper.Map<UserProfileDto>(profile);
        }
    }
}
