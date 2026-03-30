using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core repository for paper trading account queries.
    /// </summary>
    public class PaperTradingAccountRepository : FintexRepositoryBase<PaperTradingAccount, long>, IPaperTradingAccountRepository
    {
        public PaperTradingAccountRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<PaperTradingAccount> GetActiveForUserAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId && x.IsActive)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();
        }
    }
}
