using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments
{
    public interface IExternalBrokerConnectionRepository : IRepository<ExternalBrokerConnection, long>
    {
        Task<List<ExternalBrokerConnection>> GetForUserAsync(long userId);

        Task<ExternalBrokerConnection> GetByUserAndLoginAsync(
            long userId,
            ExternalBrokerProvider provider,
            string accountLogin,
            string server);

        Task<ExternalBrokerConnection> GetByIdForUserAsync(long id, long userId);
    }
}
