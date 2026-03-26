using Abp.Application.Services.Dto;
using Abp.Authorization;
using Fintex.Investments.Analytics;
using Fintex.Investments.MarketData.Dto;
using Microsoft.EntityFrameworkCore;
using System;
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
        private const int RsiPeriod = 14;
        private const int RsiWarmupCandles = 30;
        private const int VerdictBarTake = 120;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IMarketDataTimeframeCandleRepository _marketDataTimeframeCandleRepository;
        private readonly IIndicatorCalculator _indicatorCalculator;

        public MarketDataAppService(
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataTimeframeCandleRepository marketDataTimeframeCandleRepository,
            IIndicatorCalculator indicatorCalculator)
        {
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataTimeframeCandleRepository = marketDataTimeframeCandleRepository;
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

            var oneMinuteBars = await GetBarSeriesAsync(input.Symbol, input.Provider, MarketDataTimeframe.OneMinute, VerdictBarTake);
            if (oneMinuteBars.Count == 0)
            {
                return null;
            }

            var primaryCloses = oneMinuteBars.Select(x => x.Close).ToList();
            var primarySnapshot = _indicatorCalculator.Calculate(primaryCloses);
            var atr = _indicatorCalculator.CalculateAtr(oneMinuteBars);
            var adx = _indicatorCalculator.CalculateAdx(oneMinuteBars);
            var atrPercent = atr.HasValue && latest.Price > 0m
                ? decimal.Round((atr.Value / latest.Price) * 100m, 4, MidpointRounding.AwayFromZero)
                : (decimal?)null;
            var structureScore = CalculateMarketStructureScore(oneMinuteBars);
            var timeframeSignals = new List<TimeframeDirectionPoint>();

            foreach (var timeframe in SupportedVerdictTimeframes)
            {
                var bars = timeframe == MarketDataTimeframe.OneMinute
                    ? oneMinuteBars
                    : await GetBarSeriesAsync(input.Symbol, input.Provider, timeframe, VerdictBarTake);
                var timeframeSignal = CalculateTimeframeDirection(timeframe, bars);
                if (timeframeSignal != null)
                {
                    timeframeSignals.Add(timeframeSignal);
                }
            }

            var timeframeAlignmentScore = CalculateTimeframeAlignmentScore(timeframeSignals);
            var baseTrend = primarySnapshot.TrendScore ?? 0m;
            var structureTrend = decimal.Round(structureScore * 100m, 4, MidpointRounding.AwayFromZero);
            var enhancedTrend = decimal.Round(
                Clamp((baseTrend * 0.55m) + (timeframeAlignmentScore * 0.30m) + (structureTrend * 0.15m), -100m, 100m),
                4,
                MidpointRounding.AwayFromZero);

            var coverageScore = CalculateCoverageScore(primarySnapshot, timeframeSignals);
            var consensusScore = Math.Abs(enhancedTrend) * 0.22m;
            var timeframeConfidence = Math.Min(Math.Abs(timeframeAlignmentScore) * 0.18m, 18m);
            var adxBoost = CalculateAdxConfidenceBoost(adx);
            var volatilityPenalty = CalculateVolatilityPenalty(atrPercent);
            var confidenceScore = decimal.Round(
                Clamp(coverageScore + consensusScore + timeframeConfidence + adxBoost - volatilityPenalty, 0m, 100m),
                4,
                MidpointRounding.AwayFromZero);
            var verdict = CalculateEnhancedVerdict(enhancedTrend, confidenceScore);
            var indicatorScores = BuildEnhancedIndicatorScores(
                latest.Price,
                primarySnapshot,
                atrPercent,
                adx,
                structureScore,
                timeframeAlignmentScore);

            return new MarketVerdictDto
            {
                MarketDataPointId = latest.Id,
                Symbol = latest.Symbol,
                Provider = latest.Provider,
                Price = latest.Price,
                TrendScore = enhancedTrend,
                ConfidenceScore = confidenceScore,
                Verdict = verdict,
                Timestamp = latest.Timestamp,
                Sma = primarySnapshot.Sma,
                Ema = primarySnapshot.Ema,
                Rsi = primarySnapshot.Rsi,
                Macd = primarySnapshot.Macd,
                MacdSignal = primarySnapshot.MacdSignal,
                MacdHistogram = primarySnapshot.MacdHistogram,
                Momentum = primarySnapshot.Momentum,
                RateOfChange = primarySnapshot.RateOfChange,
                Atr = atr,
                AtrPercent = atrPercent,
                Adx = adx,
                StructureScore = decimal.Round(structureTrend, 4, MidpointRounding.AwayFromZero),
                StructureLabel = DescribeStructure(structureScore),
                TimeframeAlignmentScore = timeframeAlignmentScore,
                IndicatorScores = indicatorScores,
                TimeframeSignals = timeframeSignals
                    .Select(x => new MarketVerdictTimeframeDto
                    {
                        Timeframe = x.Timeframe.ToCode(),
                        BiasScore = x.BiasScore,
                        Signal = x.Signal
                    })
                    .ToList()
            };
        }

        private static readonly MarketDataTimeframe[] SupportedRsiTimeframes =
        {
            MarketDataTimeframe.OneMinute,
            MarketDataTimeframe.FiveMinutes,
            MarketDataTimeframe.FifteenMinutes,
            MarketDataTimeframe.OneHour,
            MarketDataTimeframe.FourHours
        };

        private static readonly MarketDataTimeframe[] SupportedVerdictTimeframes =
        {
            MarketDataTimeframe.OneMinute,
            MarketDataTimeframe.FiveMinutes,
            MarketDataTimeframe.FifteenMinutes,
            MarketDataTimeframe.OneHour
        };

        private async Task<List<TimeframeClosePoint>> GetClosingSeriesAsync(
            string symbol,
            MarketDataProvider provider,
            MarketDataTimeframe timeframe,
            int take)
        {
            var candles = await _marketDataTimeframeCandleRepository.GetRecentAsync(symbol, provider, timeframe, take);
            candles.Reverse();

            if (candles.Count > RsiPeriod)
            {
                return candles
                    .Select(x => new TimeframeClosePoint
                    {
                        OpenTime = x.OpenTime,
                        Close = x.Close
                    })
                    .ToList();
            }

            var fallbackStartTimeUtc = DateTime.UtcNow - TimeSpan.FromTicks(timeframe.ToTimeSpan().Ticks * RsiWarmupCandles);
            var points = await _marketDataPointRepository.GetSinceAsync(symbol, provider, fallbackStartTimeUtc);
            if (points.Count == 0)
            {
                return candles
                    .Select(x => new TimeframeClosePoint
                    {
                        OpenTime = x.OpenTime,
                        Close = x.Close
                    })
                    .ToList();
            }

            return points
                .GroupBy(x => timeframe.FloorTimestamp(x.Timestamp))
                .OrderBy(x => x.Key)
                .Select(group => new TimeframeClosePoint
                {
                    OpenTime = group.Key,
                    Close = group.Last().Price
                })
                .TakeLast(take)
                .ToList();
        }

        private async Task<List<MarketDataTimeframeCandle>> GetBarSeriesAsync(
            string symbol,
            MarketDataProvider provider,
            MarketDataTimeframe timeframe,
            int take)
        {
            var candles = await _marketDataTimeframeCandleRepository.GetRecentAsync(symbol, provider, timeframe, take);
            candles.Reverse();

            var fallbackStartTimeUtc = DateTime.UtcNow - TimeSpan.FromTicks(timeframe.ToTimeSpan().Ticks * take);
            var points = await _marketDataPointRepository.GetSinceAsync(symbol, provider, fallbackStartTimeUtc);
            if (points.Count == 0)
            {
                return candles;
            }

            var aggregated = points
                .GroupBy(x => timeframe.FloorTimestamp(x.Timestamp))
                .OrderBy(x => x.Key)
                .Select(group => BuildCandleFromPoints(group.Key, provider, timeframe, symbol, group.ToList()))
                .TakeLast(take)
                .ToList();

            return aggregated.Count >= candles.Count
                ? aggregated
                : candles;
        }

        private static MarketDataTimeframeCandle BuildCandleFromPoints(
            DateTime openTime,
            MarketDataProvider provider,
            MarketDataTimeframe timeframe,
            string symbol,
            List<MarketDataPoint> points)
        {
            var orderedPoints = points.OrderBy(x => x.Timestamp).ToList();
            var first = orderedPoints.First();
            var candle = new MarketDataTimeframeCandle(
                first.TenantId,
                provider,
                first.AssetClass,
                symbol,
                timeframe,
                openTime,
                first.Price,
                first.Timestamp);

            foreach (var point in orderedPoints.Skip(1))
            {
                candle.ApplyTick(point.Price, point.Timestamp);
            }

            return candle;
        }

        private TimeframeDirectionPoint CalculateTimeframeDirection(
            MarketDataTimeframe timeframe,
            List<MarketDataTimeframeCandle> bars)
        {
            if (bars == null || bars.Count < 3)
            {
                return null;
            }

            var closes = bars.Select(x => x.Close).ToList();
            var latestClose = closes.Last();
            var snapshot = _indicatorCalculator.Calculate(closes);
            var structureScore = CalculateMarketStructureScore(bars);
            var weightedSignals = new List<WeightedSignalPoint>();

            if (snapshot.Ema.HasValue && snapshot.Ema.Value > 0m)
            {
                var normalized = Clamp(((latestClose - snapshot.Ema.Value) / snapshot.Ema.Value) / 0.012m, -1m, 1m);
                weightedSignals.Add(new WeightedSignalPoint(36m, normalized));
            }

            if (snapshot.Rsi.HasValue)
            {
                weightedSignals.Add(new WeightedSignalPoint(18m, NormalizeRsiForVerdict(snapshot.Rsi.Value)));
            }

            if (snapshot.MacdHistogram.HasValue && latestClose > 0m)
            {
                var normalized = Clamp((snapshot.MacdHistogram.Value / latestClose) / 0.0020m, -1m, 1m);
                weightedSignals.Add(new WeightedSignalPoint(28m, normalized));
            }

            weightedSignals.Add(new WeightedSignalPoint(18m, structureScore));

            if (weightedSignals.Count == 0)
            {
                return null;
            }

            var totalWeight = weightedSignals.Sum(x => x.Weight);
            var weightedBias = weightedSignals.Sum(x => x.Weight * x.Normalized) / totalWeight;
            var biasScore = decimal.Round(Clamp(weightedBias * 100m, -100m, 100m), 4, MidpointRounding.AwayFromZero);

            return new TimeframeDirectionPoint
            {
                Timeframe = timeframe,
                BiasScore = biasScore,
                Signal = biasScore >= 12m
                    ? IndicatorSignal.Bullish
                    : biasScore <= -12m
                        ? IndicatorSignal.Bearish
                        : IndicatorSignal.Neutral
            };
        }

        private static decimal CalculateMarketStructureScore(IReadOnlyList<MarketDataTimeframeCandle> bars)
        {
            if (bars == null || bars.Count < 4)
            {
                return 0m;
            }

            var lastFour = bars.Skip(Math.Max(0, bars.Count - 4)).ToList();
            var latest = lastFour[lastFour.Count - 1];
            var previous = lastFour[lastFour.Count - 2];
            var prior = lastFour[lastFour.Count - 3];
            var score = 0m;

            if (latest.High > previous.High && previous.High > prior.High)
            {
                score += 0.35m;
            }
            else if (latest.High < previous.High && previous.High < prior.High)
            {
                score -= 0.35m;
            }

            if (latest.Low > previous.Low && previous.Low > prior.Low)
            {
                score += 0.30m;
            }
            else if (latest.Low < previous.Low && previous.Low < prior.Low)
            {
                score -= 0.30m;
            }

            var referenceBars = bars.Take(Math.Max(0, bars.Count - 1)).TakeLast(Math.Min(10, Math.Max(0, bars.Count - 1))).ToList();
            if (referenceBars.Count > 0)
            {
                var rangeHigh = referenceBars.Max(x => x.High);
                var rangeLow = referenceBars.Min(x => x.Low);

                if (latest.Close > rangeHigh)
                {
                    score += 0.30m;
                }
                else if (latest.Close < rangeLow)
                {
                    score -= 0.30m;
                }
            }

            var candleRange = latest.High - latest.Low;
            if (candleRange > 0m)
            {
                var bodyBias = (latest.Close - latest.Open) / candleRange;
                score += Clamp(bodyBias * 0.20m, -0.20m, 0.20m);
            }

            return Clamp(score, -1m, 1m);
        }

        private static string DescribeStructure(decimal structureScore)
        {
            if (structureScore >= 0.35m)
            {
                return "Bullish breakout structure";
            }

            if (structureScore <= -0.35m)
            {
                return "Bearish breakdown structure";
            }

            return "Balanced structure";
        }

        private static decimal CalculateTimeframeAlignmentScore(IReadOnlyList<TimeframeDirectionPoint> timeframeSignals)
        {
            var weightedSignals = timeframeSignals
                .Select(x => new
                {
                    Signal = x,
                    Weight = x.Timeframe == MarketDataTimeframe.OneMinute
                        ? 15m
                        : x.Timeframe == MarketDataTimeframe.FiveMinutes
                            ? 25m
                            : x.Timeframe == MarketDataTimeframe.FifteenMinutes
                                ? 30m
                                : 30m
                })
                .ToList();

            if (weightedSignals.Count == 0)
            {
                return 0m;
            }

            var totalWeight = weightedSignals.Sum(x => x.Weight);
            var weightedBias = weightedSignals.Sum(x => x.Weight * (x.Signal.BiasScore / 100m)) / totalWeight;
            return decimal.Round(Clamp(weightedBias * 100m, -100m, 100m), 4, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateCoverageScore(IndicatorSnapshot snapshot, IReadOnlyList<TimeframeDirectionPoint> timeframeSignals)
        {
            var primaryCoverage = 0m;
            primaryCoverage += snapshot.Sma.HasValue ? 6m : 0m;
            primaryCoverage += snapshot.Ema.HasValue ? 8m : 0m;
            primaryCoverage += snapshot.Rsi.HasValue ? 8m : 0m;
            primaryCoverage += snapshot.Macd.HasValue ? 8m : 0m;
            primaryCoverage += snapshot.MacdSignal.HasValue ? 6m : 0m;
            primaryCoverage += snapshot.MacdHistogram.HasValue ? 8m : 0m;
            primaryCoverage += snapshot.Momentum.HasValue ? 6m : 0m;
            primaryCoverage += snapshot.RateOfChange.HasValue ? 6m : 0m;

            var timeframeCoverage = Math.Min(timeframeSignals.Count * 5m, 20m);
            return primaryCoverage + timeframeCoverage;
        }

        private static decimal CalculateAdxConfidenceBoost(decimal? adx)
        {
            if (!adx.HasValue)
            {
                return 0m;
            }

            if (adx.Value >= 35m)
            {
                return 12m;
            }

            if (adx.Value >= 25m)
            {
                return 7m;
            }

            if (adx.Value < 15m)
            {
                return -6m;
            }

            return 0m;
        }

        private static decimal CalculateVolatilityPenalty(decimal? atrPercent)
        {
            if (!atrPercent.HasValue)
            {
                return 0m;
            }

            if (atrPercent.Value <= 0.45m)
            {
                return 0m;
            }

            return decimal.Round(Math.Min(((atrPercent.Value - 0.45m) / 0.60m) * 18m, 18m), 4, MidpointRounding.AwayFromZero);
        }

        private IReadOnlyList<IndicatorScoreDto> BuildEnhancedIndicatorScores(
            decimal latestPrice,
            IndicatorSnapshot snapshot,
            decimal? atrPercent,
            decimal? adx,
            decimal structureScore,
            decimal timeframeAlignmentScore)
        {
            var scores = new List<IndicatorScoreDto>(snapshot.Scores
                .Select(x => new IndicatorScoreDto
                {
                    Name = x.Name,
                    Value = x.Value,
                    Score = x.Score,
                    Signal = x.Signal
                }));

            if (snapshot.MacdSignal.HasValue)
            {
                scores.Add(new IndicatorScoreDto
                {
                    Name = "MACD Signal",
                    Value = snapshot.MacdSignal.Value,
                    Score = snapshot.MacdHistogram.HasValue && latestPrice > 0m
                        ? Math.Abs(Clamp((snapshot.MacdHistogram.Value / latestPrice) / 0.0020m, -1m, 1m)) * 100m
                        : 0m,
                    Signal = snapshot.Macd.HasValue && snapshot.MacdSignal.HasValue
                        ? snapshot.Macd.Value >= snapshot.MacdSignal.Value ? IndicatorSignal.Bullish : IndicatorSignal.Bearish
                        : IndicatorSignal.Neutral
                });
            }

            if (atrPercent.HasValue)
            {
                scores.Add(new IndicatorScoreDto
                {
                    Name = "ATR %",
                    Value = atrPercent.Value,
                    Score = Clamp(atrPercent.Value / 1.20m, 0m, 1m) * 100m,
                    Signal = atrPercent.Value > 0.65m ? IndicatorSignal.Caution : IndicatorSignal.Neutral
                });
            }

            if (adx.HasValue)
            {
                scores.Add(new IndicatorScoreDto
                {
                    Name = "ADX",
                    Value = adx.Value,
                    Score = Clamp(adx.Value / 40m, 0m, 1m) * 100m,
                    Signal = adx.Value >= 25m ? IndicatorSignal.Bullish : adx.Value < 15m ? IndicatorSignal.Caution : IndicatorSignal.Neutral
                });
            }

            scores.Add(new IndicatorScoreDto
            {
                Name = "Market Structure",
                Value = decimal.Round(structureScore * 100m, 4, MidpointRounding.AwayFromZero),
                Score = Math.Abs(structureScore) * 100m,
                Signal = structureScore > 0m ? IndicatorSignal.Bullish : structureScore < 0m ? IndicatorSignal.Bearish : IndicatorSignal.Neutral
            });

            scores.Add(new IndicatorScoreDto
            {
                Name = "Timeframe Alignment",
                Value = timeframeAlignmentScore,
                Score = Math.Abs(timeframeAlignmentScore),
                Signal = timeframeAlignmentScore > 0m ? IndicatorSignal.Bullish : timeframeAlignmentScore < 0m ? IndicatorSignal.Bearish : IndicatorSignal.Neutral
            });

            return scores;
        }

        private static MarketVerdict CalculateEnhancedVerdict(decimal trendScore, decimal confidenceScore)
        {
            if (confidenceScore < 40m || Math.Abs(trendScore) < 15m)
            {
                return MarketVerdict.Hold;
            }

            return trendScore > 0m ? MarketVerdict.Buy : MarketVerdict.Sell;
        }

        private static decimal NormalizeRsiForVerdict(decimal rsi)
        {
            if (rsi <= 30m)
            {
                return Clamp(((30m - rsi) / 15m) + 0.20m, 0m, 1m);
            }

            if (rsi >= 70m)
            {
                return -Clamp(((rsi - 70m) / 15m) + 0.20m, 0m, 1m);
            }

            if (rsi < 45m)
            {
                return Clamp((45m - rsi) / 30m, 0m, 0.45m);
            }

            if (rsi > 55m)
            {
                return -Clamp((rsi - 55m) / 30m, 0m, 0.45m);
            }

            return 0m;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
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

        private sealed class TimeframeClosePoint
        {
            public DateTime OpenTime { get; set; }

            public decimal Close { get; set; }
        }

        private sealed class TimeframeDirectionPoint
        {
            public MarketDataTimeframe Timeframe { get; set; }

            public decimal BiasScore { get; set; }

            public IndicatorSignal Signal { get; set; }
        }

        private sealed class WeightedSignalPoint
        {
            public WeightedSignalPoint(decimal weight, decimal normalized)
            {
                Weight = weight;
                Normalized = normalized;
            }

            public decimal Weight { get; }

            public decimal Normalized { get; }
        }
    }
}
