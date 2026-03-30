using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Notifications.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Manages in-app notifications and user-created alert rules.
    /// </summary>
    public interface INotificationAppService : IApplicationService
    {
        Task<NotificationInboxDto> GetMyInboxAsync(GetMyNotificationsInput input);

        Task<NotificationAlertRuleDto> CreatePriceAlertAsync(CreatePriceAlertInput input);

        Task SendTestAlertAsync();

        Task DeleteAlertRuleAsync(EntityDto<long> input);

        Task MarkAsReadAsync(EntityDto<long> input);

        Task MarkAllAsReadAsync();
    }
}
