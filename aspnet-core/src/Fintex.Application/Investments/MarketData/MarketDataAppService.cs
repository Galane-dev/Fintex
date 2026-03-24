using Abp.Application.Services.Dto;
using Abp.Authorization;
using Fintex.Investments.Analytics;
using Fintex.Investments.MarketData.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    /// <summary>
    /// Exposes persisted market data and indicator history to API clients.
    /// </summary>
    [AbpAuthorize]
    public class MarketDataAppService : FintexAppServiceBase, IMarketDataAppService
    {
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IIndicatorCalculator _indicatorCalculator;

        public MarketDataAppService(
            IMarketDataPointRepository marketDataPointRepository,
            IIndicatorCalculator indicatorCalculator)
        {
            _marketDataPointRepository = marketDataPointRepository;
            _indicatorCalculator = indicatorCalculator;
        }

        public async Task<MarketDataPointDto> GetLatestAsync(GetMarketDataHistoryInput input)
        {
            var entity = await _marketDataPointRepository.GetLatestAsync(input.Symbol, input.Provider);
            return entity == null ? null : ObjectMapper.Map<MarketDataPointDto>(entity);
        }

        public async Task<ListResultDto<MarketDataPointDto>> GetHistoryAsync(GetMarketDataHistoryInput input)
        {
            var normalized = input.Symbol.ToUpperInvariant();
            var history = await _marketDataPointRepository.GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == input.Provider)
                .OrderByDescending(x => x.Timestamp)
                .Take(input.Take)
                .ToListAsync();

            return new ListResultDto<MarketDataPointDto>(ObjectMapper.Map<System.Collections.Generic.List<MarketDataPointDto>>(history));
        }

        public async Task<MarketIndicatorValueDto> GetIndicatorLatestAsync(GetMarketIndicatorInput input)
        {
            var entity = await _marketDataPointRepository.GetLatestAsync(input.Symbol, input.Provider);
            return entity == null ? null : MapIndicatorValue(entity, input.Indicator);
        }

        public async Task<ListResultDto<MarketIndicatorValueDto>> GetIndicatorHistoryAsync(GetMarketIndicatorInput input)
        {
            var normalized = input.Symbol.ToUpperInvariant();
            var history = await _marketDataPointRepository.GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == input.Provider)
                .OrderByDescending(x => x.Timestamp)
                .Take(input.Take)
                .ToListAsync();

            return new ListResultDto<MarketIndicatorValueDto>(
                history.Select(x => MapIndicatorValue(x, input.Indicator)).ToList());
        }

        public Task<MarketIndicatorValueDto> GetSimpleMovingAverageLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Sma);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetSimpleMovingAverageHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Sma);
        }

        public Task<MarketIndicatorValueDto> GetExponentialMovingAverageLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Ema);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetExponentialMovingAverageHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Ema);
        }

        public Task<MarketIndicatorValueDto> GetRelativeStrengthIndexLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Rsi);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetRelativeStrengthIndexHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Rsi);
        }

        public Task<MarketIndicatorValueDto> GetStandardDeviationLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.StdDev);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetStandardDeviationHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.StdDev);
        }

        public Task<MarketIndicatorValueDto> GetMacdLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Macd);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMacdHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Macd);
        }

        public Task<MarketIndicatorValueDto> GetMacdSignalLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.MacdSignal);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMacdSignalHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.MacdSignal);
        }

        public Task<MarketIndicatorValueDto> GetMacdHistogramLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.MacdHistogram);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMacdHistogramHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.MacdHistogram);
        }

        public Task<MarketIndicatorValueDto> GetMomentumLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Momentum);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMomentumHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Momentum);
        }

        public Task<MarketIndicatorValueDto> GetRateOfChangeLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.RateOfChange);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetRateOfChangeHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.RateOfChange);
        }

        public Task<MarketIndicatorValueDto> GetBollingerUpperLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.BollingerUpper);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetBollingerUpperHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.BollingerUpper);
        }

        public Task<MarketIndicatorValueDto> GetBollingerLowerLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.BollingerLower);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetBollingerLowerHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.BollingerLower);
        }

        public Task<MarketIndicatorValueDto> GetTrendScoreLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.TrendScore);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetTrendScoreHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.TrendScore);
        }

        public Task<MarketIndicatorValueDto> GetConfidenceScoreLatestAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorLatestInternalAsync(input, MarketIndicatorType.ConfidenceScore);
        }

        public Task<ListResultDto<MarketIndicatorValueDto>> GetConfidenceScoreHistoryAsync(GetMarketDataHistoryInput input)
        {
            return GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.ConfidenceScore);
        }

        public Task<MarketVerdictDto> GetRealtimeEstimateAsync(GetMarketDataHistoryInput input)
        {
            return GetRealtimeVerdictAsync(input);
        }

        public async Task<MarketVerdictDto> GetRealtimeVerdictAsync(GetMarketDataHistoryInput input)
        {
            var latest = await _marketDataPointRepository.GetLatestAsync(input.Symbol, input.Provider);
            if (latest == null)
            {
                return null;
            }

            var recent = await _marketDataPointRepository.GetRecentAsync(input.Symbol, input.Provider, 60);
            recent.Reverse();

            var calculated = _indicatorCalculator.Calculate(recent.Select(x => x.Price).ToList());

            return new MarketVerdictDto
            {
                MarketDataPointId = latest.Id,
                Symbol = latest.Symbol,
                Provider = latest.Provider,
                Price = latest.Price,
                TrendScore = latest.TrendScore ?? calculated.TrendScore,
                ConfidenceScore = latest.ConfidenceScore ?? calculated.ConfidenceScore,
                Verdict = latest.Verdict,
                Timestamp = latest.Timestamp,
                IndicatorScores = calculated.Scores
                    .Select(x => new IndicatorScoreDto
                    {
                        Name = x.Name,
                        Value = x.Value,
                        Score = x.Score,
                        Signal = x.Signal
                    })
                    .ToList()
            };
        }

        private async Task<MarketIndicatorValueDto> GetIndicatorLatestInternalAsync(GetMarketDataHistoryInput input, MarketIndicatorType indicator)
        {
            var entity = await _marketDataPointRepository.GetLatestAsync(input.Symbol, input.Provider);
            return entity == null ? null : MapIndicatorValue(entity, indicator);
        }

        private async Task<ListResultDto<MarketIndicatorValueDto>> GetIndicatorHistoryInternalAsync(GetMarketDataHistoryInput input, MarketIndicatorType indicator)
        {
            var normalized = input.Symbol.ToUpperInvariant();
            var history = await _marketDataPointRepository.GetAll()
                .Where(x => x.Symbol == normalized && x.Provider == input.Provider)
                .OrderByDescending(x => x.Timestamp)
                .Take(input.Take)
                .ToListAsync();

            return new ListResultDto<MarketIndicatorValueDto>(
                history.Select(x => MapIndicatorValue(x, indicator)).ToList());
        }

        private static MarketIndicatorValueDto MapIndicatorValue(MarketDataPoint point, MarketIndicatorType indicator)
        {
            return new MarketIndicatorValueDto
            {
                Id = point.Id,
                Symbol = point.Symbol,
                Provider = point.Provider,
                Indicator = indicator,
                Value = GetIndicatorValue(point, indicator),
                Timestamp = point.Timestamp
            };
        }

        private static decimal? GetIndicatorValue(MarketDataPoint point, MarketIndicatorType indicator)
        {
            switch (indicator)
            {
                case MarketIndicatorType.Sma:
                    return point.Sma;
                case MarketIndicatorType.Ema:
                    return point.Ema;
                case MarketIndicatorType.Rsi:
                    return point.Rsi;
                case MarketIndicatorType.StdDev:
                    return point.StdDev;
                case MarketIndicatorType.Macd:
                    return point.Macd;
                case MarketIndicatorType.MacdSignal:
                    return point.MacdSignal;
                case MarketIndicatorType.MacdHistogram:
                    return point.MacdHistogram;
                case MarketIndicatorType.Momentum:
                    return point.Momentum;
                case MarketIndicatorType.RateOfChange:
                    return point.RateOfChange;
                case MarketIndicatorType.BollingerUpper:
                    return point.BollingerUpper;
                case MarketIndicatorType.BollingerLower:
                    return point.BollingerLower;
                case MarketIndicatorType.TrendScore:
                    return point.TrendScore;
                case MarketIndicatorType.ConfidenceScore:
                    return point.ConfidenceScore;
                default:
                    return null;
            }
        }
    }
}
