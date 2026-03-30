using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals.Services
{
    public interface IGoalExecutionService
    {
        Task<GoalExecutionResult> ExecuteAsync(GoalTarget goal, GoalPlanDraft plan, CancellationToken cancellationToken);
    }
}
