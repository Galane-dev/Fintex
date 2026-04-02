using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.Runtime.Session;
using Fintex.Authorization.Users;
using Fintex.Investments.Notifications.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Exposes the current user's notification inbox and alert-rule management.
    /// </summary>
    [AbpAuthorize]
    public class NotificationAppService : FintexAppServiceBase, INotificationAppService
    {
        private const int DefaultTake = 20;

        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly INotificationItemRepository _notificationItemRepository;
        private readonly INotificationAlertRuleRepository _notificationAlertRuleRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly INotificationEmailSender _notificationEmailSender;
        private readonly IEventBus _eventBus;

        public NotificationAppService(
            IMarketDataPointRepository marketDataPointRepository,
            INotificationItemRepository notificationItemRepository,
            INotificationAlertRuleRepository notificationAlertRuleRepository,
            IRepository<User, long> userRepository,
            INotificationEmailSender notificationEmailSender,
            IEventBus eventBus)
        {
            _marketDataPointRepository = marketDataPointRepository;
            _notificationItemRepository = notificationItemRepository;
            _notificationAlertRuleRepository = notificationAlertRuleRepository;
            _userRepository = userRepository;
            _notificationEmailSender = notificationEmailSender;
            _eventBus = eventBus;
        }

        public async Task<NotificationInboxDto> GetMyInboxAsync(GetMyNotificationsInput input)
        {
            var userId = AbpSession.GetUserId();
            var take = input?.MaxResultCount > 0 ? input.MaxResultCount : DefaultTake;
            var items = await _notificationItemRepository.GetUserNotificationsAsync(userId, take, input?.UnreadOnly ?? false);
            var rules = await _notificationAlertRuleRepository.GetUserRulesAsync(userId);

            return new NotificationInboxDto
            {
                UnreadCount = await _notificationItemRepository.GetUnreadCountAsync(userId),
                Notifications = new ListResultDto<NotificationItemDto>(items.Select(MapNotification).ToList()),
                AlertRules = new ListResultDto<NotificationAlertRuleDto>(rules.Select(MapRule).ToList())
            };
        }

        public async Task<NotificationAlertRuleDto> CreatePriceAlertAsync(CreatePriceAlertInput input)
        {
            var marketPoint = await GetLatestPointAsync(input.Symbol, input.Provider);

            if (marketPoint == null || marketPoint.Price <= 0m)
            {
                throw new Abp.UI.UserFriendlyException($"We could not find a live market price for {input.Symbol} yet.");
            }

            if (decimal.Round(marketPoint.Price, 8, MidpointRounding.AwayFromZero) == decimal.Round(input.TargetPrice, 8, MidpointRounding.AwayFromZero))
            {
                throw new Abp.UI.UserFriendlyException("Choose a target price that is different from the current market price.");
            }

            var rule = new NotificationAlertRule(
                AbpSession.TenantId,
                AbpSession.GetUserId(),
                input.Name,
                input.Symbol,
                input.Provider,
                marketPoint.Price,
                input.TargetPrice,
                input.NotifyInApp,
                input.NotifyEmail,
                input.Notes);

            await _notificationAlertRuleRepository.InsertAsync(rule);
            return MapRule(rule);
        }

        public async Task SendTestAlertAsync()
        {
            var user = await _userRepository.FirstOrDefaultAsync(AbpSession.GetUserId());
            if (user == null)
            {
                throw new Abp.UI.UserFriendlyException("We could not find your user profile for the test alert.");
            }

            var occurredAt = DateTime.UtcNow;
            var notification = new NotificationItem(
                AbpSession.TenantId,
                user.Id,
                NotificationType.TradeOpportunity,
                NotificationSeverity.Info,
                "Test alert from Fintex",
                "This is a delivery test for your in-app and email notifications.",
                "BTCUSDT",
                MarketDataProvider.Binance,
                null,
                null,
                null,
                null,
                $"test-alert:{user.Id}:{occurredAt:yyyyMMddHHmmssfff}",
                true,
                "{\"kind\":\"test-alert\"}",
                occurredAt);

            await _notificationItemRepository.InsertAsync(notification);
            await CurrentUnitOfWork.SaveChangesAsync();

            try
            {
                await _notificationEmailSender.SendAsync(user.Name, user.EmailAddress, notification.Title, BuildTestAlertEmailBody(notification));
                notification.MarkEmailSent(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                notification.MarkEmailFailed(exception.Message);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            notification.MarkInAppDelivered(DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();

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

        public async Task DeleteAlertRuleAsync(EntityDto<long> input)
        {
            var rule = await _notificationAlertRuleRepository.GetUserRuleAsync(AbpSession.GetUserId(), input.Id);
            if (rule == null)
            {
                throw new Abp.UI.UserFriendlyException("We could not find that alert rule.");
            }

            rule.Deactivate();
            await _notificationAlertRuleRepository.DeleteAsync(rule);
        }

        public async Task MarkAsReadAsync(EntityDto<long> input)
        {
            var userId = AbpSession.GetUserId();
            var notifications = await _notificationItemRepository.GetUserNotificationsAsync(userId, 200, false);
            var item = notifications.FirstOrDefault(x => x.Id == input.Id);
            if (item == null)
            {
                throw new Abp.UI.UserFriendlyException("We could not find that notification.");
            }

            item.MarkRead(DateTime.UtcNow);
            await _notificationItemRepository.UpdateAsync(item);
        }

        public async Task MarkAllAsReadAsync()
        {
            var userId = AbpSession.GetUserId();
            var notifications = await _notificationItemRepository.GetUserNotificationsAsync(userId, 200, true);
            foreach (var item in notifications.Where(x => !x.IsRead))
            {
                item.MarkRead(DateTime.UtcNow);
                await _notificationItemRepository.UpdateAsync(item);
            }
        }

        private static NotificationItemDto MapNotification(NotificationItem item)
        {
            return new NotificationItemDto
            {
                Id = item.Id,
                Type = item.Type,
                Severity = item.Severity,
                Title = item.Title,
                Message = item.Message,
                Symbol = item.Symbol,
                Provider = item.Provider,
                ReferencePrice = item.ReferencePrice,
                TargetPrice = item.TargetPrice,
                ConfidenceScore = item.ConfidenceScore,
                Verdict = item.Verdict,
                IsRead = item.IsRead,
                EmailSent = item.EmailSent,
                EmailError = item.EmailError,
                OccurredAt = item.OccurredAt.ToString("O")
            };
        }

        private static NotificationAlertRuleDto MapRule(NotificationAlertRule rule)
        {
            return new NotificationAlertRuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Symbol = rule.Symbol,
                Provider = rule.Provider,
                AlertType = rule.AlertType,
                CreatedPrice = rule.CreatedPrice,
                LastObservedPrice = rule.LastObservedPrice,
                TargetPrice = rule.TargetPrice,
                NotifyInApp = rule.NotifyInApp,
                NotifyEmail = rule.NotifyEmail,
                IsActive = rule.IsActive,
                Notes = rule.Notes,
                CreationTime = rule.CreationTime.ToString("O"),
                LastTriggeredAt = rule.LastTriggeredAt?.ToString("O")
            };
        }

        private async Task<MarketDataPoint> GetLatestPointAsync(string symbol, MarketDataProvider provider)
        {
            MarketDataPoint latestPoint;
            var alternateSymbol = GetAlternateMarketSymbol(symbol, provider);

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                latestPoint = await _marketDataPointRepository.GetLatestAsync(symbol, provider);
                if (latestPoint == null && !string.IsNullOrWhiteSpace(alternateSymbol))
                {
                    latestPoint = await _marketDataPointRepository.GetLatestAsync(alternateSymbol, provider);
                }

                if (latestPoint == null)
                {
                    latestPoint = await _marketDataPointRepository.GetLatestBySymbolAsync(symbol);
                }

                if (latestPoint == null && !string.IsNullOrWhiteSpace(alternateSymbol))
                {
                    latestPoint = await _marketDataPointRepository.GetLatestBySymbolAsync(alternateSymbol);
                }
            }

            return latestPoint;
        }

        private static string GetAlternateMarketSymbol(string symbol, MarketDataProvider provider)
        {
            if (provider != MarketDataProvider.Binance || string.IsNullOrWhiteSpace(symbol))
            {
                return null;
            }

            var normalized = symbol.Trim().ToUpperInvariant().Replace("/", string.Empty, StringComparison.Ordinal);
            if (normalized.EndsWith("USD", StringComparison.Ordinal) &&
                !normalized.EndsWith("USDT", StringComparison.Ordinal))
            {
                return normalized.Substring(0, normalized.Length - 3) + "USDT";
            }

            return null;
        }

        private static string BuildTestAlertEmailBody(NotificationItem notification)
        {
            return NotificationEmailTemplateBuilder.Build(
                notification,
                "This is a test alert to confirm your Fintex email notification delivery is working correctly.");
        }
    }
}
