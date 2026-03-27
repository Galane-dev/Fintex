using Abp.Domain.Repositories;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for user profile aggregate lookups.
    /// </summary>
    public interface IUserProfileRepository : IRepository<UserProfile, long>
    {
        Task<UserProfile> GetByUserIdAsync(long userId);
    }
}
