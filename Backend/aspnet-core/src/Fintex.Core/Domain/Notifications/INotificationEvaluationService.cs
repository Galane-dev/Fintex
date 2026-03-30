using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// Evaluates market updates against system opportunity rules and user alert rules.
    /// </summary>
    public interface INotificationEvaluationService
    {
        Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken);
    }
}
