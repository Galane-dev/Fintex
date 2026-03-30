using Fintex.Investments.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Automation
{
    /// <summary>
    /// Evaluates live market updates against active trade automation rules.
    /// </summary>
    public interface ITradeAutomationEvaluationService
    {
        Task EvaluateAsync(NotificationMarketSnapshot snapshot, CancellationToken cancellationToken);
    }
}
