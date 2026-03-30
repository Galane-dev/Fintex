using Fintex.Investments.Analytics;
using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
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

            if (latest.High > previous.High && previous.High > prior.High) score += 0.35m;
            else if (latest.High < previous.High && previous.High < prior.High) score -= 0.35m;

            if (latest.Low > previous.Low && previous.Low > prior.Low) score += 0.30m;
            else if (latest.Low < previous.Low && previous.Low < prior.Low) score -= 0.30m;

            var referenceBars = bars.Take(Math.Max(0, bars.Count - 1)).TakeLast(Math.Min(10, Math.Max(0, bars.Count - 1))).ToList();
            if (referenceBars.Count > 0)
            {
                var rangeHigh = referenceBars.Max(x => x.High);
                var rangeLow = referenceBars.Min(x => x.Low);

                if (latest.Close > rangeHigh) score += 0.30m;
                else if (latest.Close < rangeLow) score -= 0.30m;
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
            if (structureScore >= 0.35m) return "Bullish breakout structure";
            if (structureScore <= -0.35m) return "Bearish breakdown structure";
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
                            : x.Timeframe == MarketDataTimeframe.FifteenMinutes ? 30m : 30m
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
            if (!adx.HasValue) return 0m;
            if (adx.Value >= 35m) return 12m;
            if (adx.Value >= 25m) return 7m;
            if (adx.Value < 15m) return -6m;
            return 0m;
        }

        private static decimal CalculateVolatilityPenalty(decimal? atrPercent)
        {
            if (!atrPercent.HasValue || atrPercent.Value <= 0.45m)
            {
                return 0m;
            }

            return decimal.Round(
                Math.Min(((atrPercent.Value - 0.45m) / 0.60m) * 18m, 18m),
                4,
                MidpointRounding.AwayFromZero);
        }

        private IReadOnlyList<IndicatorScoreDto> BuildEnhancedIndicatorScores(
            decimal latestPrice,
            IndicatorSnapshot snapshot,
            decimal? atrPercent,
            decimal? adx,
            decimal structureScore,
            decimal timeframeAlignmentScore)
        {
            var scores = new List<IndicatorScoreDto>(snapshot.Scores.Select(x => new IndicatorScoreDto
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
            if (rsi <= 30m) return Clamp(((30m - rsi) / 15m) + 0.20m, 0m, 1m);
            if (rsi >= 70m) return -Clamp(((rsi - 70m) / 15m) + 0.20m, 0m, 1m);
            if (rsi < 45m) return Clamp((45m - rsi) / 30m, 0m, 0.45m);
            if (rsi > 55m) return -Clamp((rsi - 55m) / 30m, 0m, 0.45m);
            return 0m;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
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
                case MarketIndicatorType.Sma: return point.Sma;
                case MarketIndicatorType.Ema: return point.Ema;
                case MarketIndicatorType.Rsi: return point.Rsi;
                case MarketIndicatorType.StdDev: return point.StdDev;
                case MarketIndicatorType.Macd: return point.Macd;
                case MarketIndicatorType.MacdSignal: return point.MacdSignal;
                case MarketIndicatorType.MacdHistogram: return point.MacdHistogram;
                case MarketIndicatorType.Momentum: return point.Momentum;
                case MarketIndicatorType.RateOfChange: return point.RateOfChange;
                case MarketIndicatorType.BollingerUpper: return point.BollingerUpper;
                case MarketIndicatorType.BollingerLower: return point.BollingerLower;
                case MarketIndicatorType.TrendScore: return point.TrendScore;
                case MarketIndicatorType.ConfidenceScore: return point.ConfidenceScore;
                default: return null;
            }
        }
    }
}
