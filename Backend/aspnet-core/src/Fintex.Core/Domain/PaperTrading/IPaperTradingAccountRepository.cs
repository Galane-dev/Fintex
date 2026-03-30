using Abp.Domain.Repositories;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for paper trading account queries.
    /// </summary>
    public interface IPaperTradingAccountRepository : IRepository<PaperTradingAccount, long>
    {
        Task<PaperTradingAccount> GetActiveForUserAsync(long userId);
    }
}
