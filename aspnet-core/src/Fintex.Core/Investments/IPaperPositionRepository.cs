using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    /// <summary>
    /// Repository contract for querying paper positions.
    /// </summary>
    public interface IPaperPositionRepository : IRepository<PaperPosition, long>
    {
        Task<List<PaperPosition>> GetOpenPositionsAsync(long accountId);

        Task<PaperPosition> GetOpenBySymbolAsync(long accountId, string symbol, MarketDataProvider provider);
    }
}
