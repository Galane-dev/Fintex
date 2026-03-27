using Abp.Application.Services.Dto;
using Fintex.Investments.MarketData.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
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

            return new ListResultDto<MarketDataPointDto>(
                ObjectMapper.Map<List<MarketDataPointDto>>(history));
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

        public Task<MarketIndicatorValueDto> GetSimpleMovingAverageLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Sma);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetSimpleMovingAverageHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Sma);

        public Task<MarketIndicatorValueDto> GetExponentialMovingAverageLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Ema);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetExponentialMovingAverageHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Ema);

        public Task<MarketIndicatorValueDto> GetRelativeStrengthIndexLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Rsi);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetRelativeStrengthIndexHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Rsi);

        public async Task<ListResultDto<MarketTimeframeRsiDto>> GetRelativeStrengthIndexTimeframesAsync(GetMarketDataHistoryInput input)
        {
            var items = new List<MarketTimeframeRsiDto>();

            foreach (var timeframe in SupportedRsiTimeframes)
            {
                var candleSeries = await GetClosingSeriesAsync(input.Symbol, input.Provider, timeframe, RsiWarmupCandles);
                items.Add(new MarketTimeframeRsiDto
                {
                    Timeframe = timeframe.ToCode(),
                    Value = _indicatorCalculator.CalculateRsi(candleSeries.Select(x => x.Close).ToList(), RsiPeriod),
                    CandleTimestamp = candleSeries.LastOrDefault()?.OpenTime
                });
            }

            return new ListResultDto<MarketTimeframeRsiDto>(items);
        }

        public Task<MarketIndicatorValueDto> GetStandardDeviationLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.StdDev);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetStandardDeviationHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.StdDev);

        public Task<MarketIndicatorValueDto> GetMacdLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Macd);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMacdHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Macd);

        public Task<MarketIndicatorValueDto> GetMacdSignalLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.MacdSignal);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMacdSignalHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.MacdSignal);

        public Task<MarketIndicatorValueDto> GetMacdHistogramLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.MacdHistogram);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMacdHistogramHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.MacdHistogram);

        public Task<MarketIndicatorValueDto> GetMomentumLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.Momentum);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetMomentumHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.Momentum);

        public Task<MarketIndicatorValueDto> GetRateOfChangeLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.RateOfChange);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetRateOfChangeHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.RateOfChange);

        public Task<MarketIndicatorValueDto> GetBollingerUpperLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.BollingerUpper);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetBollingerUpperHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.BollingerUpper);

        public Task<MarketIndicatorValueDto> GetBollingerLowerLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.BollingerLower);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetBollingerLowerHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.BollingerLower);

        public Task<MarketIndicatorValueDto> GetTrendScoreLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.TrendScore);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetTrendScoreHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.TrendScore);

        public Task<MarketIndicatorValueDto> GetConfidenceScoreLatestAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorLatestInternalAsync(input, MarketIndicatorType.ConfidenceScore);

        public Task<ListResultDto<MarketIndicatorValueDto>> GetConfidenceScoreHistoryAsync(GetMarketDataHistoryInput input) =>
            GetIndicatorHistoryInternalAsync(input, MarketIndicatorType.ConfidenceScore);

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
    }
}
