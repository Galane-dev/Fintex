using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Persists and delivers user notifications through the configured channels.
    /// </summary>
    public interface INotificationDispatchService
    {
        Task<NotificationItem> DispatchAsync(NotificationDispatchRequest request);
    }
}
