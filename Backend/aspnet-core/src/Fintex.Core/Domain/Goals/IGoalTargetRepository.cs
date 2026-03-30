using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals
{
    public interface IGoalTargetRepository : IRepository<GoalTarget, long>
    {
        Task<List<GoalTarget>> GetActiveGoalsAsync(string marketSymbol);

        Task<List<GoalTarget>> GetUserGoalsAsync(long userId);

        Task<GoalTarget> GetUserGoalAsync(long userId, long goalId);
    }
}
