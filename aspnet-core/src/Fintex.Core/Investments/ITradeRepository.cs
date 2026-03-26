using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for trade aggregate-specific queries.
    /// </summary>
    public interface ITradeRepository : IRepository<Trade, long>
    {
        Task<List<Trade>> GetOpenTradesBySymbolAsync(string symbol);

        Task<List<Trade>> GetUserTradesAsync(long userId);

        Task<List<Trade>> GetUserOpenTradesAsync(long userId);

        Task<Trade> GetByExternalOrderIdAsync(long userId, string externalOrderId);
    }
}
