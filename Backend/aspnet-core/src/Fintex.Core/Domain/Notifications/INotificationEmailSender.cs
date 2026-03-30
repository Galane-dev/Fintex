using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Infrastructure abstraction for email delivery of user notifications.
    /// </summary>
    public interface INotificationEmailSender
    {
        Task SendAsync(string recipientName, string recipientEmail, string subject, string htmlBody);
    }
}
