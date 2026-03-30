using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Automation.Dto;
using Fintex.Investments.Brokers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.Automation
{
    /// <summary>
    /// Creates and manages one-shot auto-execution rules for the signed-in user.
    /// </summary>
    [AbpAuthorize]
    public class TradeAutomationAppService : FintexAppServiceBase, ITradeAutomationAppService
    {
        private readonly ITradeAutomationRuleRepository _tradeAutomationRuleRepository;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;

        public TradeAutomationAppService(
            ITradeAutomationRuleRepository tradeAutomationRuleRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository)
        {
            _tradeAutomationRuleRepository = tradeAutomationRuleRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
        }

        public async Task<ListResultDto<TradeAutomationRuleDto>> GetMyRulesAsync()
        {
            var rules = await _tradeAutomationRuleRepository.GetUserRulesAsync(AbpSession.GetUserId());
            return new ListResultDto<TradeAutomationRuleDto>(rules.Select(MapRule).ToList());
        }

        public async Task<TradeAutomationRuleDto> CreateRuleAsync(CreateTradeAutomationRuleInput input)
        {
            await ValidateDestinationAsync(input);

            var currentMetricValue = await GetCurrentMetricValueAsync(input);
            if (currentMetricValue.HasValue && input.TriggerValue.HasValue && currentMetricValue.Value == Round(input.TriggerValue.Value))
            {
                throw new UserFriendlyException("Choose a trigger value that differs from the current live reading.");
            }

            var rule = new TradeAutomationRule(
                AbpSession.TenantId,
                AbpSession.GetUserId(),
                input.Name,
                input.Symbol,
                input.Provider,
                input.TriggerType,
                currentMetricValue,
                input.TriggerValue,
                input.TargetVerdict,
                input.MinimumConfidenceScore,
                input.Destination,
                input.ExternalConnectionId,
                input.TradeDirection,
                input.Quantity,
                input.StopLoss,
                input.TakeProfit,
                input.NotifyInApp,
                input.NotifyEmail,
                input.Notes);

            await _tradeAutomationRuleRepository.InsertAsync(rule);
            await CurrentUnitOfWork.SaveChangesAsync();
            return MapRule(rule);
        }

        public async Task DeleteRuleAsync(EntityDto<long> input)
        {
            var rule = await _tradeAutomationRuleRepository.GetUserRuleAsync(AbpSession.GetUserId(), input.Id);
            if (rule == null)
            {
                throw new UserFriendlyException("We could not find that automation rule.");
            }

            rule.Deactivate();
            await _tradeAutomationRuleRepository.DeleteAsync(rule);
        }

        private async Task ValidateDestinationAsync(CreateTradeAutomationRuleInput input)
        {
            if (input.Destination != TradeAutomationDestination.ExternalBroker)
            {
                return;
            }

            if (!input.ExternalConnectionId.HasValue)
            {
                throw new UserFriendlyException("Choose the external broker account that should execute this rule.");
            }

            var connection = await _externalBrokerConnectionRepository.GetByIdForUserAsync(
                input.ExternalConnectionId.Value,
                AbpSession.GetUserId());
            if (connection == null || !connection.IsActive)
            {
                throw new UserFriendlyException("The selected external broker account is not available.");
            }
        }

        private async Task<decimal?> GetCurrentMetricValueAsync(CreateTradeAutomationRuleInput input)
        {
            if (input.TriggerType == TradeAutomationTriggerType.Verdict)
            {
                return null;
            }

            var latestPoint = await GetLatestPointAsync(input.Symbol, input.Provider);
            if (latestPoint == null)
            {
                throw new UserFriendlyException($"We could not find a live market reading for {input.Symbol} yet.");
            }

            var currentValue = input.TriggerType switch
            {
                TradeAutomationTriggerType.PriceTarget => latestPoint.Price,
                TradeAutomationTriggerType.RelativeStrengthIndex => latestPoint.Rsi,
                TradeAutomationTriggerType.MacdHistogram => latestPoint.MacdHistogram,
                TradeAutomationTriggerType.Momentum => latestPoint.Momentum,
                TradeAutomationTriggerType.TrendScore => latestPoint.TrendScore,
                TradeAutomationTriggerType.ConfidenceScore => latestPoint.ConfidenceScore,
                _ => null
            };

            if (!currentValue.HasValue)
            {
                throw new UserFriendlyException("The current market reading for this trigger is still loading. Please try again shortly.");
            }

            return Round(currentValue.Value);
        }

        private async Task<MarketDataPoint> GetLatestPointAsync(string symbol, MarketDataProvider provider)
        {
            MarketDataPoint latestPoint;
            var alternateSymbol = GetAlternateMarketSymbol(symbol, provider);

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                latestPoint = await _marketDataPointRepository.GetLatestAsync(symbol, provider)
                    ?? (alternateSymbol == null ? null : await _marketDataPointRepository.GetLatestAsync(alternateSymbol, provider))
                    ?? await _marketDataPointRepository.GetLatestBySymbolAsync(symbol)
                    ?? (alternateSymbol == null ? null : await _marketDataPointRepository.GetLatestBySymbolAsync(alternateSymbol));
            }

            return latestPoint;
        }

        private static string GetAlternateMarketSymbol(string symbol, MarketDataProvider provider)
        {
            if (provider != MarketDataProvider.Binance || string.IsNullOrWhiteSpace(symbol))
            {
                return null;
            }

            var normalized = symbol.Trim().ToUpperInvariant().Replace("/", string.Empty);
            return normalized.EndsWith("USD") && !normalized.EndsWith("USDT")
                ? normalized.Substring(0, normalized.Length - 3) + "USDT"
                : null;
        }

        private static TradeAutomationRuleDto MapRule(TradeAutomationRule rule)
        {
            return new TradeAutomationRuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Symbol = rule.Symbol,
                Provider = rule.Provider,
                TriggerType = rule.TriggerType,
                CreatedMetricValue = rule.CreatedMetricValue,
                LastObservedMetricValue = rule.LastObservedMetricValue,
                TriggerValue = rule.TargetMetricValue,
                TargetVerdict = rule.TargetVerdict,
                MinimumConfidenceScore = rule.MinimumConfidenceScore,
                Destination = rule.Destination,
                ExternalConnectionId = rule.ExternalConnectionId,
                TradeDirection = rule.TradeDirection,
                Quantity = rule.Quantity,
                StopLoss = rule.StopLoss,
                TakeProfit = rule.TakeProfit,
                NotifyInApp = rule.NotifyInApp,
                NotifyEmail = rule.NotifyEmail,
                IsActive = rule.IsActive,
                Notes = rule.Notes,
                CreationTime = rule.CreationTime.ToString("O"),
                LastTriggeredAt = rule.LastTriggeredAt?.ToString("O"),
                LastTradeId = rule.LastTradeId
            };
        }

        private static decimal Round(decimal value)
        {
            return decimal.Round(value, 8, System.MidpointRounding.AwayFromZero);
        }
    }
}
