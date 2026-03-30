using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.Investments.Goals;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    public class GoalTargetRepository : FintexRepositoryBase<GoalTarget, long>, IGoalTargetRepository
    {
        public GoalTargetRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<GoalTarget>> GetActiveGoalsAsync(string marketSymbol)
        {
            var normalizedSymbol = Normalize(marketSymbol);
            return await GetAll()
                .Where(x => x.MarketSymbol == normalizedSymbol && (x.Status == GoalStatus.Accepted || x.Status == GoalStatus.Active))
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<List<GoalTarget>> GetUserGoalsAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<GoalTarget> GetUserGoalAsync(long userId, long goalId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId && x.Id == goalId)
                .FirstOrDefaultAsync();
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
