using Abp.Application.Services;
using Fintex.Investments.Brokers.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    /// <summary>
    /// Application contract for live order routing through external brokers.
    /// </summary>
    public interface IExternalBrokerTradingAppService : IApplicationService
    {
        Task<ExternalBrokerTradeExecutionDto> PlaceMarketOrderAsync(PlaceExternalBrokerMarketOrderInput input);

        Task<ExternalBrokerSyncResultDto> SyncMyConnectionsAsync();
    }
}
