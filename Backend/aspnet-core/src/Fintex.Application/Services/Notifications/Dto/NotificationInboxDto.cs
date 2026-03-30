using Abp.Application.Services.Dto;
using System.Collections.Generic;

namespace Fintex.Investments.Notifications.Dto
{
    /// <summary>
    /// Notification inbox payload returned to the frontend.
    /// </summary>
    public class NotificationInboxDto
    {
        public int UnreadCount { get; set; }

        public ListResultDto<NotificationItemDto> Notifications { get; set; }

        public ListResultDto<NotificationAlertRuleDto> AlertRules { get; set; }
    }
}
