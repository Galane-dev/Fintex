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
    /// EF Core repository for querying paper trade fills.
    /// </summary>
    public class PaperTradeFillRepository : FintexRepositoryBase<PaperTradeFill, long>, IPaperTradeFillRepository
    {
        public PaperTradeFillRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<PaperTradeFill>> GetAccountFillsAsync(long accountId)
        {
            return await GetAll()
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.ExecutedAt)
                .ToListAsync();
        }
    }
}
