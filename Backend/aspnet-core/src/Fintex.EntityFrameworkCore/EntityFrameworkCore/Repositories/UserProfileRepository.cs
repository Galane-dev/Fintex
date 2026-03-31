using Abp.EntityFrameworkCore;
using Fintex.EntityFrameworkCore.Repositories;
using Fintex.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// EF Core repository for user profile lookups.
    /// </summary>
    public class UserProfileRepository : FintexRepositoryBase<UserProfile, long>, IUserProfileRepository
    {
        public UserProfileRepository(IDbContextProvider<FintexDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public Task<UserProfile> GetByUserIdAsync(long userId)
        {
            return GetAll().FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}
