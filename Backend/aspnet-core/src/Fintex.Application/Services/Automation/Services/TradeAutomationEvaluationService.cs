using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Investments.Brokers;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Notifications;
using Fintex.Investments.PaperTrading;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Automation
{
    /// <summary>
    /// Executes one-shot automation rules when live market updates satisfy their trigger conditions.
    /// </summary>
    public class TradeAutomationEvaluationService : ITradeAutomationEvaluationService, ITransientDependency
    {
        private readonly ITradeAutomationRuleRepository _tradeAutomationRuleRepository;
        private readonly IPaperTradingAppService _paperTradingAppService;
        private readonly IExternalBrokerTradingAppService _externalBrokerTradingAppService;
        private readonly INotificationDispatchService _notificationDispatchService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAbpSession _abpSession;

        public TradeAutomationEvaluationService(
            ITradeAutomationRuleRepository tradeAutomationRuleRepository,
            IPaperTradingAppService paperTradingAppService,
            IExternalBrokerTradingAppService externalBrokerTradingAppService,
            INotificationDispatchService notificationDispatchService,
            IUnitOfWorkManager unitOfWorkManager,
            IAbpSession abpSession)
        {
            _tradeAutomationRuleRepository = tradeAutomationRuleRepository;
            _paperTradingAppService = paperTradingAppService;
            _externalBrokerTradingAppService = externalBrokerTradingAppService;
            _notificationDispatchService = notificationDispatchService;
            _unitOfWorkManager = unitOfWorkManager;
            _abpSession = abpSession;
        }

        public async Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.Symbol) || snapshot.Price <= 0m)
            {
                return;
            }

            List<TradeAutomationRule> rules;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                rules = await _tradeAutomationRuleRepository.GetActiveRulesAsync(snapshot.Symbol, snapshot.Provider);
            }

            foreach (var rule in rules)
            {
                if (!rule.ShouldTrigger(snapshot))
                {
                    if (rule.RefreshObservedMetric(snapshot))
                    {
                        await _tradeAutomationRuleRepository.UpdateAsync(rule);
                    }

                    continue;
                }

                await ExecuteRuleAsync(rule, snapshot);
            }
        }

        private async Task ExecuteRuleAsync(TradeAutomationRule rule, NotificationMarketSnapshot snapshot)
        {
            var occurredAt = DateTime.UtcNow;
            var observedMetric = rule.GetMetricValue(snapshot);

            try
            {
                var execution = await ExecuteTradeAsync(rule);
                var notification = await _notificationDispatchService.DispatchAsync(new NotificationDispatchRequest
                {
                    TenantId = rule.TenantId,
                    UserId = rule.UserId,
                    Type = NotificationType.TradeAutomation,
                    Severity = NotificationSeverity.Success,
                    Title = BuildSuccessTitle(rule),
                    Message = execution.Summary,
                    Symbol = rule.Symbol,
                    Provider = rule.Provider,
                    ReferencePrice = snapshot.Price,
                    TargetPrice = rule.TargetMetricValue,
                    ConfidenceScore = snapshot.ConfidenceScore,
                    Verdict = snapshot.Verdict,
                    TriggerKey = BuildTriggerKey(rule),
                    NotifyInApp = rule.NotifyInApp,
                    NotifyEmail = rule.NotifyEmail,
                    ContextJson = BuildContextJson(rule, execution, null),
                    OccurredAt = occurredAt
                });
                rule.Trigger(notification.Id, execution.TradeId, occurredAt, observedMetric);
                await _tradeAutomationRuleRepository.UpdateAsync(rule);
            }
            catch (Exception exception)
            {
                var notification = await _notificationDispatchService.DispatchAsync(new NotificationDispatchRequest
                {
                    TenantId = rule.TenantId,
                    UserId = rule.UserId,
                    Type = NotificationType.TradeAutomation,
                    Severity = NotificationSeverity.Warning,
                    Title = BuildFailureTitle(rule),
                    Message = BuildFailureMessage(rule, exception),
                    Symbol = rule.Symbol,
                    Provider = rule.Provider,
                    ReferencePrice = snapshot.Price,
                    TargetPrice = rule.TargetMetricValue,
                    ConfidenceScore = snapshot.ConfidenceScore,
                    Verdict = snapshot.Verdict,
                    TriggerKey = BuildTriggerKey(rule),
                    NotifyInApp = rule.NotifyInApp,
                    NotifyEmail = rule.NotifyEmail,
                    ContextJson = BuildContextJson(rule, null, exception.Message),
                    OccurredAt = occurredAt
                });
                rule.Trigger(notification.Id, null, occurredAt, observedMetric);
                await _tradeAutomationRuleRepository.UpdateAsync(rule);
            }
        }

        private async Task<(long? TradeId, string Summary)> ExecuteTradeAsync(TradeAutomationRule rule)
        {
            using (_abpSession.Use(rule.TenantId, rule.UserId))
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (rule.Destination == TradeAutomationDestination.PaperTrading)
                {
                    var execution = await _paperTradingAppService.PlaceMarketOrderAsync(new PlacePaperOrderInput
                    {
                        Symbol = rule.Symbol,
                        AssetClass = AssetClass.Crypto,
                        Provider = rule.Provider,
                        Direction = rule.TradeDirection,
                        Quantity = rule.Quantity,
                        StopLoss = rule.StopLoss,
                        TakeProfit = rule.TakeProfit,
                        Notes = BuildExecutionNotes(rule)
                    });

                    if (!execution.WasExecuted)
                    {
                        throw new UserFriendlyException(execution.Assessment?.Headline ?? "The paper trade was blocked.");
                    }

                    return (execution.Order?.Id, $"Fintex auto-executed a paper {rule.TradeDirection.ToString().ToLowerInvariant()} on {rule.Symbol} after {DescribeTrigger(rule)}.");
                }

                var executionResult = await _externalBrokerTradingAppService.PlaceMarketOrderAsync(new PlaceExternalBrokerMarketOrderInput
                {
                    ConnectionId = rule.ExternalConnectionId ?? 0,
                    Symbol = rule.Symbol,
                    AssetClass = AssetClass.Crypto,
                    Provider = rule.Provider,
                    Direction = rule.TradeDirection,
                    Quantity = rule.Quantity,
                    StopLoss = rule.StopLoss,
                    TakeProfit = rule.TakeProfit,
                    Notes = BuildExecutionNotes(rule)
                });

                return (executionResult.Trade?.Id, executionResult.Summary);
            }
        }

        private static string BuildSuccessTitle(TradeAutomationRule rule)
        {
            return $"Auto trade executed: {rule.Name}";
        }

        private static string BuildFailureTitle(TradeAutomationRule rule)
        {
            return $"Auto trade failed: {rule.Name}";
        }

        private static string BuildFailureMessage(TradeAutomationRule rule, Exception exception)
        {
            return $"Fintex detected {DescribeTrigger(rule)}, but the {rule.TradeDirection.ToString().ToLowerInvariant()} order could not be placed: {exception.Message}";
        }

        private static string BuildExecutionNotes(TradeAutomationRule rule)
        {
            return string.IsNullOrWhiteSpace(rule.Notes)
                ? $"Auto execution from rule '{rule.Name}'."
                : $"{rule.Notes} | Auto execution from rule '{rule.Name}'.";
        }

        private static string BuildTriggerKey(TradeAutomationRule rule)
        {
            return $"automation:{rule.Id}:{rule.Symbol}:{rule.TriggerType}:{rule.TargetMetricValue?.ToString("0.########") ?? rule.TargetVerdict?.ToString() ?? "trigger"}";
        }

        private static string DescribeTrigger(TradeAutomationRule rule)
        {
            return rule.TriggerType == TradeAutomationTriggerType.Verdict
                ? $"the market verdict turning {rule.TargetVerdict?.ToString().ToLowerInvariant()} with confidence {rule.MinimumConfidenceScore?.ToString("0.##") ?? "any"}"
                : $"{rule.TriggerType} crossing {rule.TargetMetricValue?.ToString("0.########") ?? "-"}";
        }

        private static string BuildContextJson(TradeAutomationRule rule, (long? TradeId, string Summary)? execution, string error)
        {
            return $"{{\"ruleId\":{rule.Id},\"triggerType\":\"{rule.TriggerType}\",\"tradeId\":{execution?.TradeId?.ToString() ?? "null"},\"destination\":\"{rule.Destination}\",\"error\":{(error == null ? "null" : $"\"{error.Replace("\"", "'")}\"")}}}";
        }
    }
}
