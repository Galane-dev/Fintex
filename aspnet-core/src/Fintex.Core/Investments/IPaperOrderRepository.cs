using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for querying paper orders.
    /// </summary>
    public interface IPaperOrderRepository : IRepository<PaperOrder, long>
    {
        Task<List<PaperOrder>> GetUserOrdersAsync(long userId, long accountId);
    }
}
