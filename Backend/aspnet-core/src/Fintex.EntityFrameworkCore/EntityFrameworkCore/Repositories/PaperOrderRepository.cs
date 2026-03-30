using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core repository for querying paper orders.
    /// </summary>
    public class PaperOrderRepository : FintexRepositoryBase<PaperOrder, long>, IPaperOrderRepository
    {
        public PaperOrderRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<PaperOrder>> GetUserOrdersAsync(long userId, long accountId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId && x.AccountId == accountId)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();
        }
    }
}
