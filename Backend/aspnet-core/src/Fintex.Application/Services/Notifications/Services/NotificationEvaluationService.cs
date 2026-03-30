using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Creates and delivers notifications for explicit user alert rules.
    /// </summary>
    public class NotificationEvaluationService : INotificationEvaluationService
    {
        private readonly INotificationAlertRuleRepository _notificationAlertRuleRepository;
        private readonly INotificationDispatchService _notificationDispatchService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public NotificationEvaluationService(
            INotificationAlertRuleRepository notificationAlertRuleRepository,
            INotificationDispatchService notificationDispatchService,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _notificationAlertRuleRepository = notificationAlertRuleRepository;
            _notificationDispatchService = notificationDispatchService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.Symbol) || snapshot.Price <= 0m)
            {
                return;
            }

            await EvaluatePriceAlertsAsync(snapshot);
        }

        private async Task EvaluatePriceAlertsAsync(NotificationMarketSnapshot snapshot)
        {
            System.Collections.Generic.List<NotificationAlertRule> rules;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                rules = await _notificationAlertRuleRepository.GetActivePriceAlertsAsync(snapshot.Symbol, snapshot.Provider);
            }

            if (rules.Count == 0)
            {
                return;
            }

            foreach (var rule in rules)
            {
                var hasCrossedTarget = rule.ShouldTrigger(snapshot.Price, snapshot.Bid, snapshot.Ask);
                if (!hasCrossedTarget)
                {
                    if (rule.RefreshObservedPrice(snapshot.Price))
                    {
                        await _notificationAlertRuleRepository.UpdateAsync(rule);
                    }

                    continue;
                }

                var notification = await _notificationDispatchService.DispatchAsync(new NotificationDispatchRequest
                {
                    TenantId = rule.TenantId,
                    UserId = rule.UserId,
                    Type = NotificationType.PriceTarget,
                    Severity = NotificationSeverity.Warning,
                    Title = $"BTC alert hit: {rule.Name}",
                    Message = BuildPriceTargetMessage(rule, snapshot.Price),
                    Symbol = rule.Symbol,
                    Provider = rule.Provider,
                    ReferencePrice = snapshot.Price,
                    TargetPrice = rule.TargetPrice,
                    ConfidenceScore = snapshot.ConfidenceScore,
                    Verdict = snapshot.Verdict,
                    TriggerKey = BuildPriceAlertTriggerKey(rule),
                    NotifyInApp = rule.NotifyInApp,
                    NotifyEmail = rule.NotifyEmail,
                    ContextJson = BuildPriceAlertContextJson(rule, snapshot.Price),
                    OccurredAt = DateTime.UtcNow
                });
                rule.Trigger(notification.Id, notification.OccurredAt, snapshot.Price);
                await _notificationAlertRuleRepository.UpdateAsync(rule);
            }
        }

        private static string BuildPriceAlertTriggerKey(NotificationAlertRule rule)
        {
            return $"price:{rule.Id}:{rule.Symbol}:{rule.CreatedPrice?.ToString("0.########") ?? "legacy"}:{rule.TargetPrice.ToString("0.########")}";
        }

        private static string BuildPriceTargetMessage(NotificationAlertRule rule, decimal currentPrice)
        {
            if (rule.LastObservedPrice.HasValue && rule.LastObservedPrice.Value > 0m)
            {
                return $"{rule.Symbol} crossed your target of {rule.TargetPrice.ToString("0.00")} between {rule.LastObservedPrice.Value.ToString("0.00")} and {currentPrice.ToString("0.00")}.";
            }

            if (rule.CreatedPrice.HasValue && rule.CreatedPrice.Value > 0m)
            {
                return $"{rule.Symbol} crossed your target of {rule.TargetPrice.ToString("0.00")} from the alert start price of {rule.CreatedPrice.Value.ToString("0.00")} and is now trading near {currentPrice.ToString("0.00")}.";
            }

            return $"{rule.Symbol} moved {rule.Direction.ToString().ToLowerInvariant()} your target of {rule.TargetPrice.ToString("0.00")} and is now trading near {currentPrice.ToString("0.00")}.";
        }

        private static string BuildPriceAlertContextJson(NotificationAlertRule rule, decimal currentPrice)
        {
            return $"{{\"ruleId\":{rule.Id},\"targetPrice\":{rule.TargetPrice},\"currentPrice\":{currentPrice},\"createdPrice\":{rule.CreatedPrice?.ToString("0.########") ?? "null"},\"lastObservedPrice\":{rule.LastObservedPrice?.ToString("0.########") ?? "null"}}}";
        }
    }
}
