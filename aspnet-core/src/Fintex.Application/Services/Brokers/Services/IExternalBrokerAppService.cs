using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Brokers.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    public interface IExternalBrokerAppService : IApplicationService
    {
        Task<ListResultDto<ExternalBrokerConnectionDto>> GetMyConnectionsAsync();

        Task<ExternalBrokerConnectionDto> ConnectAlpacaAccountAsync(ConnectAlpacaBrokerAccountInput input);

        Task DisconnectAsync(EntityDto<long> input);
    }
}
