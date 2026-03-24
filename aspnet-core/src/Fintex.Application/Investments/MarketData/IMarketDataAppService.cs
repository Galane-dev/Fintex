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

        Task<MarketIndicatorValueDto> GetIndicatorLatestAsync(GetMarketIndicatorInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetIndicatorHistoryAsync(GetMarketIndicatorInput input);

        Task<MarketIndicatorValueDto> GetSimpleMovingAverageLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetSimpleMovingAverageHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetExponentialMovingAverageLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetExponentialMovingAverageHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetRelativeStrengthIndexLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetRelativeStrengthIndexHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetStandardDeviationLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetStandardDeviationHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetMacdLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetMacdHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetMacdSignalLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetMacdSignalHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetMacdHistogramLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetMacdHistogramHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetMomentumLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetMomentumHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetRateOfChangeLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetRateOfChangeHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetBollingerUpperLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetBollingerUpperHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetBollingerLowerLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetBollingerLowerHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetTrendScoreLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetTrendScoreHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketIndicatorValueDto> GetConfidenceScoreLatestAsync(GetMarketDataHistoryInput input);

        Task<ListResultDto<MarketIndicatorValueDto>> GetConfidenceScoreHistoryAsync(GetMarketDataHistoryInput input);

        Task<MarketVerdictDto> GetRealtimeEstimateAsync(GetMarketDataHistoryInput input);

        Task<MarketVerdictDto> GetRealtimeVerdictAsync(GetMarketDataHistoryInput input);
    }
}
