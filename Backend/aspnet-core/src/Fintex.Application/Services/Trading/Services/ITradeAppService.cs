using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Trading.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Trading
{
    /// <summary>
    /// Application contract for trade management.
    /// </summary>
    public interface ITradeAppService : IApplicationService
    {
        Task<ListResultDto<TradeDto>> GetMyTradesAsync();

        Task<TradeDto> GetAsync(EntityDto<long> input);

        Task<TradeDto> CreateAsync(CreateTradeInput input);

        Task<TradeDto> CloseAsync(CloseTradeInput input);
    }
}
