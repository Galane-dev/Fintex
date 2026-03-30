using Abp.Application.Services.Dto;

namespace Fintex.Investments.Notifications.Dto
{
    /// <summary>
    /// Query options for the current user's notification inbox.
    /// </summary>
    public class GetMyNotificationsInput : PagedResultRequestDto
    {
        public bool UnreadOnly { get; set; }
    }
}
