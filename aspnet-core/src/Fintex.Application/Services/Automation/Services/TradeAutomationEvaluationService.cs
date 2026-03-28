using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.Runtime.Session;
using Abp.UI;
using Fintex.Authorization.Users;
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
        private readonly IRepository<User, long> _userRepository;
        private readonly INotificationItemRepository _notificationItemRepository;
        private readonly INotificationEmailSender _notificationEmailSender;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAbpSession _abpSession;

        public TradeAutomationEvaluationService(
            ITradeAutomationRuleRepository tradeAutomationRuleRepository,
            IPaperTradingAppService paperTradingAppService,
            IExternalBrokerTradingAppService externalBrokerTradingAppService,
            IRepository<User, long> userRepository,
            INotificationItemRepository notificationItemRepository,
            INotificationEmailSender notificationEmailSender,
            IEventBus eventBus,
            IUnitOfWorkManager unitOfWorkManager,
            IAbpSession abpSession)
        {
            _tradeAutomationRuleRepository = tradeAutomationRuleRepository;
            _paperTradingAppService = paperTradingAppService;
            _externalBrokerTradingAppService = externalBrokerTradingAppService;
            _userRepository = userRepository;
            _notificationItemRepository = notificationItemRepository;
            _notificationEmailSender = notificationEmailSender;
            _eventBus = eventBus;
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
                var notification = new NotificationItem(
                    rule.TenantId,
                    rule.UserId,
                    NotificationType.TradeAutomation,
                    NotificationSeverity.Success,
                    BuildSuccessTitle(rule),
                    execution.Summary,
                    rule.Symbol,
                    rule.Provider,
                    snapshot.Price,
                    rule.TargetMetricValue,
                    snapshot.ConfidenceScore,
                    snapshot.Verdict,
                    BuildTriggerKey(rule),
                    rule.NotifyEmail,
                    BuildContextJson(rule, execution, null),
                    occurredAt);

                await PersistAndDeliverAsync(notification, rule, occurredAt);
                rule.Trigger(notification.Id, execution.TradeId, occurredAt, observedMetric);
                await _tradeAutomationRuleRepository.UpdateAsync(rule);
            }
            catch (Exception exception)
            {
                var notification = new NotificationItem(
                    rule.TenantId,
                    rule.UserId,
                    NotificationType.TradeAutomation,
                    NotificationSeverity.Warning,
                    BuildFailureTitle(rule),
                    BuildFailureMessage(rule, exception),
                    rule.Symbol,
                    rule.Provider,
                    snapshot.Price,
                    rule.TargetMetricValue,
                    snapshot.ConfidenceScore,
                    snapshot.Verdict,
                    BuildTriggerKey(rule),
                    rule.NotifyEmail,
                    BuildContextJson(rule, null, exception.Message),
                    occurredAt);

                await PersistAndDeliverAsync(notification, rule, occurredAt);
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

        private async Task PersistAndDeliverAsync(NotificationItem notification, TradeAutomationRule rule, DateTime occurredAt)
        {
            await _notificationItemRepository.InsertAsync(notification);
            await _unitOfWorkManager.Current.SaveChangesAsync();

            var user = await GetUserAsync(rule.UserId);
            if (rule.NotifyEmail && user != null)
            {
                await DeliverEmailAsync(notification, user.Name, user.EmailAddress);
            }

            if (!rule.NotifyInApp)
            {
                return;
            }

            notification.MarkInAppDelivered(occurredAt);
            await _unitOfWorkManager.Current.SaveChangesAsync();

            await _eventBus.TriggerAsync(new NotificationCreatedEventData
            {
                NotificationId = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Symbol = notification.Symbol,
                Severity = notification.Severity.ToString(),
                Type = notification.Type.ToString(),
                ConfidenceScore = notification.ConfidenceScore,
                OccurredAt = notification.OccurredAt
            });
        }

        private async Task DeliverEmailAsync(NotificationItem notification, string recipientName, string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                return;
            }

            try
            {
                await _notificationEmailSender.SendAsync(recipientName, recipientEmail, notification.Title, BuildEmailBody(notification));
                notification.MarkEmailSent(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                notification.MarkEmailFailed(exception.Message);
            }

            await _unitOfWorkManager.Current.SaveChangesAsync();
        }

        private async Task<User> GetUserAsync(long userId)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                return await _userRepository.FirstOrDefaultAsync(userId);
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

        private static string BuildEmailBody(NotificationItem notification)
        {
            return $"<h2>{notification.Title}</h2><p>{notification.Message}</p><p><strong>Occurred:</strong> {notification.OccurredAt:O}</p>";
        }
    }
}
