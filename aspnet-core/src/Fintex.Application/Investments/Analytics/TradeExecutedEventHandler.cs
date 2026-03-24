using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Fintex.Investments.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Refreshes user behavioral insights when trades are executed.
    /// </summary>
    public class TradeExecutedEventHandler : IAsyncEventHandler<TradeExecutedEventData>, ITransientDependency
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IBehavioralAnalysisClient _behavioralAnalysisClient;

        public TradeExecutedEventHandler(
            IUserProfileRepository userProfileRepository,
            ITradeRepository tradeRepository,
            IBehavioralAnalysisClient behavioralAnalysisClient)
        {
            _userProfileRepository = userProfileRepository;
            _tradeRepository = tradeRepository;
            _behavioralAnalysisClient = behavioralAnalysisClient;
        }

        public async Task HandleEventAsync(TradeExecutedEventData eventData)
        {
            if (eventData.UserId <= 0)
            {
                return;
            }

            var profile = await _userProfileRepository.GetByUserIdAsync(eventData.UserId);
            if (profile == null || !profile.IsAiInsightsEnabled)
            {
                return;
            }

            var recentTrades = await _tradeRepository.GetAll()
                .Where(x => x.UserId == eventData.UserId)
                .OrderByDescending(x => x.CreationTime)
                .Take(20)
                .ToListAsync();

            var insight = await _behavioralAnalysisClient.AnalyzeAsync(profile, recentTrades, CancellationToken.None);
            if (!insight.WasGenerated)
            {
                return;
            }

            profile.ApplyBehavioralInsight(insight.RiskScore, insight.Summary, insight.Provider, insight.Model, DateTime.UtcNow);
            await _userProfileRepository.UpdateAsync(profile);
        }
    }
}
