using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    public class ExternalBrokerConnectionRepository : FintexRepositoryBase<ExternalBrokerConnection, long>, IExternalBrokerConnectionRepository
    {
        public ExternalBrokerConnectionRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<ExternalBrokerConnection>> GetForUserAsync(long userId)
        {
            return await GetAll()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.LastValidatedAt)
                .ThenByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<ExternalBrokerConnection> GetByUserAndLoginAsync(
            long userId,
            ExternalBrokerProvider provider,
            string accountLogin,
            string server)
        {
            return await GetAll()
                .Where(x =>
                    x.UserId == userId &&
                    x.Provider == provider &&
                    x.AccountLogin == accountLogin &&
                    x.Server == server)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();
        }

        public async Task<ExternalBrokerConnection> GetByIdForUserAsync(long id, long userId)
        {
            return await GetAll()
                .Where(x => x.Id == id && x.UserId == userId)
                .FirstOrDefaultAsync();
        }
    }
}
