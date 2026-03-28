using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.News;
using Fintex.Investments.Profiles;
using Fintex.Investments.Profiles.Dto;
using Fintex.Investments.Strategies.Dto;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// Validates user strategies against current market, news, and profile context.
    /// </summary>
    [AbpAuthorize]
    public class StrategyValidationAppService : FintexAppServiceBase, IStrategyValidationAppService
    {
        private readonly IRepository<StrategyValidationRun, long> _strategyValidationRepository;
        private readonly IMarketDataAppService _marketDataAppService;
        private readonly IUserProfileAppService _userProfileAppService;
        private readonly INewsRecommendationService _newsRecommendationService;
        private readonly IStrategyValidationClient _strategyValidationClient;

        public StrategyValidationAppService(
            IRepository<StrategyValidationRun, long> strategyValidationRepository,
            IMarketDataAppService marketDataAppService,
            IUserProfileAppService userProfileAppService,
            INewsRecommendationService newsRecommendationService,
            IStrategyValidationClient strategyValidationClient)
        {
            _strategyValidationRepository = strategyValidationRepository;
            _marketDataAppService = marketDataAppService;
            _userProfileAppService = userProfileAppService;
            _newsRecommendationService = newsRecommendationService;
            _strategyValidationClient = strategyValidationClient;
        }

        public async Task<StrategyValidationResultDto> ValidateMyStrategyAsync(ValidateStrategyInput input)
        {
            var userId = AbpSession.GetUserId();
            var run = new StrategyValidationRun(
                AbpSession.TenantId,
                userId,
                input.StrategyName,
                NormalizeSymbol(input.Symbol),
                input.Provider,
                input.Timeframe,
                input.DirectionPreference,
                input.StrategyText);

            var marketVerdict = await LoadMarketVerdictAsync(run.Symbol, run.Provider);
            var newsInsight = await _newsRecommendationService.GetBitcoinUsdInsightAsync(marketVerdict, CancellationToken.None);
            var profile = await _userProfileAppService.GetMyProfileAsync();

            var insight = await _strategyValidationClient.ValidateAsync(
                new ValidateStrategyRequest
                {
                    StrategyName = input.StrategyName,
                    Symbol = run.Symbol,
                    Timeframe = input.Timeframe,
                    DirectionPreference = input.DirectionPreference,
                    StrategyText = input.StrategyText
                },
                marketVerdict,
                newsInsight,
                profile,
                CancellationToken.None);

            run.ApplyResult(
                marketVerdict?.Price,
                marketVerdict?.TrendScore,
                marketVerdict?.ConfidenceScore,
                marketVerdict?.Verdict.ToString(),
                newsInsight?.Summary,
                insight.ValidationScore,
                insight.Outcome,
                insight.Summary,
                JsonConvert.SerializeObject(insight.Strengths),
                JsonConvert.SerializeObject(insight.Risks),
                JsonConvert.SerializeObject(insight.Improvements),
                insight.SuggestedAction,
                insight.SuggestedEntryPrice,
                insight.SuggestedStopLoss,
                insight.SuggestedTakeProfit,
                insight.Provider,
                insight.Model);

            await _strategyValidationRepository.InsertAsync(run);
            await CurrentUnitOfWork.SaveChangesAsync();

            return MapResult(run);
        }

        public async Task<ListResultDto<StrategyValidationResultDto>> GetMyHistoryAsync(int take = 8)
        {
            var userId = AbpSession.GetUserId();
            var limit = take <= 0 ? 8 : take > 20 ? 20 : take;
            var items = await _strategyValidationRepository.GetAllListAsync();

            return new ListResultDto<StrategyValidationResultDto>(
                items.Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreationTime)
                    .Take(limit)
                    .Select(MapResult)
                    .ToList());
        }

        private Task<MarketVerdictDto> LoadMarketVerdictAsync(string symbol, MarketDataProvider provider)
        {
            return _marketDataAppService.GetRealtimeVerdictAsync(new GetMarketDataHistoryInput
            {
                Symbol = symbol,
                Provider = provider,
                Take = 80
            });
        }

        private static StrategyValidationResultDto MapResult(StrategyValidationRun run)
        {
            return new StrategyValidationResultDto
            {
                Id = run.Id,
                CreationTime = run.CreationTime,
                CreatorUserId = run.CreatorUserId,
                LastModificationTime = run.LastModificationTime,
                LastModifierUserId = run.LastModifierUserId,
                IsDeleted = run.IsDeleted,
                DeleterUserId = run.DeleterUserId,
                DeletionTime = run.DeletionTime,
                StrategyName = run.StrategyName,
                Symbol = run.Symbol,
                Timeframe = run.Timeframe,
                DirectionPreference = run.DirectionPreference,
                StrategyText = run.StrategyText,
                MarketPrice = run.MarketPrice,
                MarketTrendScore = run.MarketTrendScore,
                MarketConfidenceScore = run.MarketConfidenceScore,
                MarketVerdict = run.MarketVerdict,
                NewsSummary = run.NewsSummary,
                ValidationScore = run.ValidationScore ?? 0m,
                Outcome = run.Outcome,
                Summary = run.Summary,
                Strengths = ParseList(run.StrengthsJson),
                Risks = ParseList(run.RisksJson),
                Improvements = ParseList(run.ImprovementsJson),
                SuggestedAction = run.SuggestedAction,
                SuggestedEntryPrice = run.SuggestedEntryPrice,
                SuggestedStopLoss = run.SuggestedStopLoss,
                SuggestedTakeProfit = run.SuggestedTakeProfit,
                AiProvider = run.AiProvider,
                AiModel = run.AiModel
            };
        }

        private static List<string> ParseList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(value) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }

        private static string NormalizeSymbol(string symbol) =>
            string.IsNullOrWhiteSpace(symbol) ? "BTCUSDT" : symbol.Trim().ToUpperInvariant();
    }
}
