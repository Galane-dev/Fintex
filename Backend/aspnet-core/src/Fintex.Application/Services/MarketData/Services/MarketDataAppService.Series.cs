using Fintex.Investments.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    public partial class MarketDataAppService
    {
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
                    .Select(x => new TimeframeClosePoint { OpenTime = x.OpenTime, Close = x.Close })
                    .ToList();
            }

            var fallbackStartTimeUtc = DateTime.UtcNow - TimeSpan.FromTicks(timeframe.ToTimeSpan().Ticks * RsiWarmupCandles);
            var points = await _marketDataPointRepository.GetSinceAsync(symbol, provider, fallbackStartTimeUtc);
            if (points.Count == 0)
            {
                return candles
                    .Select(x => new TimeframeClosePoint { OpenTime = x.OpenTime, Close = x.Close })
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

            return aggregated.Count >= candles.Count ? aggregated : candles;
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
    }
}
