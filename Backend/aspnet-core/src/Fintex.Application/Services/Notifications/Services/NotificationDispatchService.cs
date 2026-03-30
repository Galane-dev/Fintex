using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Fintex.Authorization.Users;
using System;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Shared delivery service for in-app and email notifications.
    /// </summary>
    public class NotificationDispatchService : INotificationDispatchService, ITransientDependency
    {
        private readonly INotificationItemRepository _notificationItemRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly INotificationEmailSender _notificationEmailSender;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public NotificationDispatchService(
            INotificationItemRepository notificationItemRepository,
            IRepository<User, long> userRepository,
            INotificationEmailSender notificationEmailSender,
            IEventBus eventBus,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _notificationItemRepository = notificationItemRepository;
            _userRepository = userRepository;
            _notificationEmailSender = notificationEmailSender;
            _eventBus = eventBus;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<NotificationItem> DispatchAsync(NotificationDispatchRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var notification = new NotificationItem(
                request.TenantId,
                request.UserId,
                request.Type,
                request.Severity,
                request.Title,
                request.Message,
                request.Symbol,
                request.Provider,
                request.ReferencePrice,
                request.TargetPrice,
                request.ConfidenceScore,
                request.Verdict,
                request.TriggerKey,
                request.NotifyEmail,
                request.ContextJson,
                request.OccurredAt);

            await _notificationItemRepository.InsertAsync(notification);
            await _unitOfWorkManager.Current.SaveChangesAsync();

            if (request.NotifyEmail)
            {
                var user = await GetUserAsync(request.UserId);
                if (!string.IsNullOrWhiteSpace(user?.EmailAddress))
                {
                    await DeliverEmailAsync(notification, user.Name, user.EmailAddress);
                }
            }

            if (request.NotifyInApp)
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

            return notification;
        }

        private async Task<User> GetUserAsync(long userId)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                return await _userRepository.FirstOrDefaultAsync(userId);
            }
        }

        private async Task DeliverEmailAsync(NotificationItem notification, string recipientName, string recipientEmail)
        {
            try
            {
                await _notificationEmailSender.SendAsync(
                    recipientName,
                    recipientEmail,
                    notification.Title,
                    $"<h2>{notification.Title}</h2><p>{notification.Message}</p><p><strong>Symbol:</strong> {notification.Symbol}</p><p><strong>Occurred:</strong> {notification.OccurredAt:O}</p>");
                notification.MarkEmailSent(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                notification.MarkEmailFailed(exception.Message);
            }

            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
