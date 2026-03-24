using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.MarketData.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    /// <summary>
    /// Application contract for market data queries.
    /// </summary>
    public interface IMarketDataAppService : IApplicationService
    {
        Task<MarketDataPointDto> GetLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketDataPointDto>> GetHistoryAsync(GetMarketDataHistoryInput input);
    }
}
