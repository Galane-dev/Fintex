using Fintex.Investments.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals.Services
{
    public interface IGoalMonitoringService
    {
        Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken);
    }
}
