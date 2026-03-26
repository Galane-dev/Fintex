using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.PaperTrading.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    /// <summary>
    /// Contract for the internal paper trading simulator.
    /// </summary>
    public interface IPaperTradingAppService : IApplicationService
    {
        Task<PaperTradingAccountDto> CreateMyAccountAsync(CreatePaperTradingAccountInput input);

        Task<PaperTradingSnapshotDto> GetMySnapshotAsync();

        Task<ListResultDto<PaperOrderDto>> GetMyOrdersAsync();

        Task<ListResultDto<PaperPositionDto>> GetMyPositionsAsync();

        Task<ListResultDto<PaperTradeFillDto>> GetMyFillsAsync();

        Task<PaperTradeExecutionResultDto> PlaceMarketOrderAsync(PlacePaperOrderInput input);

        Task<PaperTradeRecommendationDto> GetRecommendationAsync(GetPaperTradeRecommendationInput input);

        Task<PaperOrderDto> ClosePositionAsync(ClosePaperPositionInput input);
    }
}
