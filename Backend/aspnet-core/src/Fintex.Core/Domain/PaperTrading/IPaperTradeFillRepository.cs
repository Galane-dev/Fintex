using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for querying paper trade fills.
    /// </summary>
    public interface IPaperTradeFillRepository : IRepository<PaperTradeFill, long>
    {
        Task<List<PaperTradeFill>> GetAccountFillsAsync(long accountId);
    }
}
