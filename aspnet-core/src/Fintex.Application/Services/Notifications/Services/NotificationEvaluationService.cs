using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Fintex.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Creates and delivers notifications for price alerts and high-confidence opportunities.
    /// </summary>
    public class NotificationEvaluationService : INotificationEvaluationService
    {
        private const decimal OpportunityConfidenceThreshold = 80m;
        private static readonly TimeSpan OpportunityCooldown = TimeSpan.FromMinutes(15);

        private readonly INotificationAlertRuleRepository _notificationAlertRuleRepository;
        private readonly INotificationItemRepository _notificationItemRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly INotificationEmailSender _notificationEmailSender;
        private readonly IEventBus _eventBus;

        public NotificationEvaluationService(
            INotificationAlertRuleRepository notificationAlertRuleRepository,
            INotificationItemRepository notificationItemRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<User, long> userRepository,
            INotificationEmailSender notificationEmailSender,
            IEventBus eventBus)
        {
            _notificationAlertRuleRepository = notificationAlertRuleRepository;
            _notificationItemRepository = notificationItemRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _userRepository = userRepository;
            _notificationEmailSender = notificationEmailSender;
            _eventBus = eventBus;
        }

        public async Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.Symbol) || snapshot.Price <= 0m)
            {
                return;
            }

            await EvaluateOpportunityAlertsAsync(snapshot);
            await EvaluatePriceAlertsAsync(snapshot);
        }

        private async Task EvaluateOpportunityAlertsAsync(NotificationMarketSnapshot snapshot)
        {
            if (snapshot.Verdict == MarketVerdict.Hold || !snapshot.ConfidenceScore.HasValue || snapshot.ConfidenceScore.Value < OpportunityConfidenceThreshold)
            {
                return;
            }

            List<User> users;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                users = await _userRepository.GetAllListAsync(x => x.IsActive);
            }

            var occurredAt = DateTime.UtcNow;
            var triggerKey = BuildOpportunityTriggerKey(snapshot);

            foreach (var user in users)
            {
                if (await _notificationItemRepository.ExistsRecentAsync(user.Id, triggerKey, occurredAt.Subtract(OpportunityCooldown)))
                {
                    continue;
                }

                var notification = new NotificationItem(
                    user.TenantId,
                    user.Id,
                    NotificationType.TradeOpportunity,
                    NotificationSeverity.Success,
                    BuildOpportunityTitle(snapshot),
                    BuildOpportunityMessage(snapshot),
                    snapshot.Symbol,
                    snapshot.Provider,
                    snapshot.Price,
                    null,
                    snapshot.ConfidenceScore,
                    snapshot.Verdict,
                    triggerKey,
                    true,
                    BuildOpportunityContextJson(snapshot),
                    occurredAt);

                await PersistAndDeliverAsync(notification, user.Name, user.EmailAddress);
            }
        }

        private async Task EvaluatePriceAlertsAsync(NotificationMarketSnapshot snapshot)
        {
            List<NotificationAlertRule> rules;
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

                var notification = new NotificationItem(
                    rule.TenantId,
                    rule.UserId,
                    NotificationType.PriceTarget,
                    NotificationSeverity.Warning,
                    $"BTC alert hit: {rule.Name}",
                    BuildPriceTargetMessage(rule, snapshot.Price),
                    rule.Symbol,
                    rule.Provider,
                    snapshot.Price,
                    rule.TargetPrice,
                    snapshot.ConfidenceScore,
                    snapshot.Verdict,
                    BuildPriceAlertTriggerKey(rule),
                    rule.NotifyEmail,
                    BuildPriceAlertContextJson(rule, snapshot.Price),
                    DateTime.UtcNow);

                await _notificationItemRepository.InsertAsync(notification);
                await _unitOfWorkManager.Current.SaveChangesAsync();
                rule.Trigger(notification.Id, notification.OccurredAt, snapshot.Price);
                await _notificationAlertRuleRepository.UpdateAsync(rule);

                var user = await GetUserAsync(rule.UserId);
                if (rule.NotifyEmail && user != null)
                {
                    await DeliverEmailAsync(notification, user.Name, user.EmailAddress);
                }

                await PublishAsync(notification);
            }
        }

        private async Task PersistAndDeliverAsync(NotificationItem notification, string recipientName, string recipientEmail)
        {
            await _notificationItemRepository.InsertAsync(notification);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            await DeliverEmailAsync(notification, recipientName, recipientEmail);
            await PublishAsync(notification);
        }

        private async Task DeliverEmailAsync(NotificationItem notification, string recipientName, string recipientEmail)
        {
            if (!notification.EmailRequested || string.IsNullOrWhiteSpace(recipientEmail))
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

        private async Task PublishAsync(NotificationItem notification)
        {
            notification.MarkInAppDelivered(DateTime.UtcNow);
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

        private async Task<User> GetUserAsync(long userId)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                return await _userRepository.FirstOrDefaultAsync(userId);
            }
        }

        private static string BuildOpportunityTriggerKey(NotificationMarketSnapshot snapshot)
        {
            return $"opportunity:{snapshot.Symbol}:{snapshot.Provider}:{snapshot.Verdict}";
        }

        private static string BuildPriceAlertTriggerKey(NotificationAlertRule rule)
        {
            return $"price:{rule.Id}:{rule.Symbol}:{rule.CreatedPrice?.ToString("0.########") ?? "legacy"}:{rule.TargetPrice.ToString("0.########")}";
        }

        private static string BuildOpportunityTitle(NotificationMarketSnapshot snapshot)
        {
            return $"High-confidence {snapshot.Verdict.ToString().ToLowerInvariant()} setup spotted";
        }

        private static string BuildOpportunityMessage(NotificationMarketSnapshot snapshot)
        {
            var confidence = snapshot.ConfidenceScore.HasValue
                ? snapshot.ConfidenceScore.Value.ToString("0.0")
                : "-";

            return $"{snapshot.Symbol} is showing a {snapshot.Verdict.ToString().ToLowerInvariant()} opportunity with confidence {confidence} near {snapshot.Price.ToString("0.00")}.";
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

        private static string BuildOpportunityContextJson(NotificationMarketSnapshot snapshot)
        {
            return $"{{\"price\":{snapshot.Price},\"verdict\":\"{snapshot.Verdict}\",\"confidence\":{snapshot.ConfidenceScore?.ToString("0.####") ?? "null"},\"trendScore\":{snapshot.TrendScore?.ToString("0.####") ?? "null"}}}";
        }

        private static string BuildPriceAlertContextJson(NotificationAlertRule rule, decimal currentPrice)
        {
            return $"{{\"ruleId\":{rule.Id},\"targetPrice\":{rule.TargetPrice},\"currentPrice\":{currentPrice},\"createdPrice\":{rule.CreatedPrice?.ToString("0.########") ?? "null"},\"lastObservedPrice\":{rule.LastObservedPrice?.ToString("0.########") ?? "null"}}}";
        }

        private static string BuildEmailBody(NotificationItem notification)
        {
            return $"<h2>{notification.Title}</h2><p>{notification.Message}</p><p><strong>Symbol:</strong> {notification.Symbol}</p><p><strong>Occurred:</strong> {notification.OccurredAt:O}</p>";
        }
    }
}
