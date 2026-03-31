using Abp.Authorization;
using Fintex.Investments.Assistant.Dto;
using Fintex.Investments.Brokers;
using Fintex.Investments.Goals.Services;
using Fintex.Investments.MarketData;
using Fintex.Investments.Notifications;
using Fintex.Investments.PaperTrading;
using Fintex.Investments.Profiles;
using Fintex.Investments.Trading;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fintex.Investments.Assistant
{
    /// <summary>
    /// Orchestrates AI-backed chat and action execution against Fintex features.
    /// </summary>
    [AbpAuthorize]
    public partial class AssistantAppService : FintexAppServiceBase, IAssistantAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMarketDataAppService _marketDataAppService;
        private readonly IPaperTradingAppService _paperTradingAppService;
        private readonly INotificationAppService _notificationAppService;
        private readonly IUserProfileAppService _userProfileAppService;
        private readonly ITradeAppService _tradeAppService;
        private readonly IExternalBrokerAppService _externalBrokerAppService;
        private readonly IExternalBrokerTradingAppService _externalBrokerTradingAppService;
        private readonly IGoalAutomationAppService _goalAutomationAppService;

        public AssistantAppService(
            HttpClient httpClient,
            IConfiguration configuration,
            IMarketDataAppService marketDataAppService,
            IPaperTradingAppService paperTradingAppService,
            INotificationAppService notificationAppService,
            IUserProfileAppService userProfileAppService,
            ITradeAppService tradeAppService,
            IExternalBrokerAppService externalBrokerAppService,
            IExternalBrokerTradingAppService externalBrokerTradingAppService,
            IGoalAutomationAppService goalAutomationAppService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _marketDataAppService = marketDataAppService;
            _paperTradingAppService = paperTradingAppService;
            _notificationAppService = notificationAppService;
            _userProfileAppService = userProfileAppService;
            _tradeAppService = tradeAppService;
            _externalBrokerAppService = externalBrokerAppService;
            _externalBrokerTradingAppService = externalBrokerTradingAppService;
            _goalAutomationAppService = goalAutomationAppService;
        }

        public async Task<AssistantChatResponseDto> SendMessageAsync(AssistantChatInput input)
        {
            var snapshot = await LoadContextSnapshotAsync();
            var plan = await BuildPlanAsync(input, snapshot);
            var actionResults = await ExecuteActionsAsync(plan.Actions);

            return new AssistantChatResponseDto
            {
                Reply = string.IsNullOrWhiteSpace(plan.Reply)
                    ? "I'm ready to help with trades, alerts, recommendations, and explaining the dashboard."
                    : plan.Reply,
                VoiceReply = string.IsNullOrWhiteSpace(plan.VoiceReply) ? plan.Reply : plan.VoiceReply,
                UsedAi = true,
                Provider = "OpenAI",
                Model = _configuration["OpenAI:Model"],
                SuggestedPrompts = plan.SuggestedPrompts.Any() ? plan.SuggestedPrompts : BuildFallbackPrompts(snapshot),
                ActionResults = actionResults
            };
        }

        private static List<string> BuildFallbackPrompts(AssistantContextSnapshot snapshot)
        {
            return new List<string>
            {
                "Explain the current verdict and confidence.",
                "Set a BTC alert at 70000 and email me.",
                "Give me a trade recommendation right now.",
                snapshot.Goals.Any() ? "List my active BTC goals." : "Create a BTC growth goal for my paper account by tomorrow afternoon.",
            };
        }
    }
}
