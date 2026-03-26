using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Fintex.Investments.Analytics;
using Fintex.Investments.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.MarketData
{
    /// <summary>
    /// Converts inbound market ticks into persisted market data points and trade analytics.
    /// </summary>
    public class MarketDataIngestionService : IMarketDataIngestionService, ITransientDependency
    {
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IMarketDataTimeframeCandleRepository _marketDataTimeframeCandleRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IIndicatorCalculator _indicatorCalculator;
        private readonly TradeAnalysisService _tradeAnalysisService;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MarketDataIngestionService(
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataTimeframeCandleRepository marketDataTimeframeCandleRepository,
            ITradeRepository tradeRepository,
            IIndicatorCalculator indicatorCalculator,
            TradeAnalysisService tradeAnalysisService,
            IEventBus eventBus,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataTimeframeCandleRepository = marketDataTimeframeCandleRepository;
            _tradeRepository = tradeRepository;
            _indicatorCalculator = indicatorCalculator;
            _tradeAnalysisService = tradeAnalysisService;
            _eventBus = eventBus;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<MarketDataPoint> IngestAsync(MarketStreamTick tick, CancellationToken cancellationToken)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                foreach (var timeframe in SupportedTimeframes)
                {
                    await UpsertTimeframeCandleAsync(tick, timeframe);
                }

                await _unitOfWorkManager.Current.SaveChangesAsync();

                var recentCandles = await _marketDataTimeframeCandleRepository.GetRecentAsync(
                    tick.Symbol,
                    tick.Provider,
                    MarketDataTimeframe.OneMinute,
                    120);
                recentCandles.Reverse();

                var priceSeries = new List<decimal>(recentCandles.Select(x => x.Close));
                var indicators = _indicatorCalculator.Calculate(priceSeries);

                var point = new MarketDataPoint(
                    tick.TenantId,
                    tick.Provider,
                    tick.AssetClass,
                    tick.Symbol,
                    tick.Price,
                    tick.Bid,
                    tick.Ask,
                    tick.Volume,
                    tick.Open24Hours,
                    tick.High24Hours,
                    tick.Low24Hours,
                    tick.Sequence,
                    tick.Timestamp);

                point.ApplyIndicators(
                    indicators.Sma,
                    indicators.Ema,
                    indicators.Rsi,
                    indicators.StdDev,
                    indicators.Macd,
                    indicators.MacdSignal,
                    indicators.MacdHistogram,
                    indicators.Momentum,
                    indicators.RateOfChange,
                    indicators.BollingerUpper,
                    indicators.BollingerLower,
                    indicators.TrendScore,
                    indicators.ConfidenceScore,
                    indicators.Verdict);

                await _marketDataPointRepository.InsertAsync(point);
                await _unitOfWorkManager.Current.SaveChangesAsync();

                var openTrades = await _tradeRepository.GetOpenTradesBySymbolAsync(point.Symbol);
                foreach (var trade in openTrades)
                {
                    trade.RefreshMarketPrice(point.Price);
                    await _tradeAnalysisService.AnalyzeAndPersistAsync(trade, point, cancellationToken);
                }

                await _eventBus.TriggerAsync(new MarketDataUpdatedEventData
                {
                    TenantId = tick.TenantId,
                    MarketDataPointId = point.Id,
                    Symbol = point.Symbol,
                    AssetClass = point.AssetClass,
                    Provider = point.Provider,
                    Price = point.Price,
                    Bid = point.Bid,
                    Ask = point.Ask,
                    Volume = point.Volume,
                    Sma = point.Sma,
                    Ema = point.Ema,
                    Rsi = point.Rsi,
                    StdDev = point.StdDev,
                    Macd = point.Macd,
                    MacdSignal = point.MacdSignal,
                    MacdHistogram = point.MacdHistogram,
                    Momentum = point.Momentum,
                    RateOfChange = point.RateOfChange,
                    BollingerUpper = point.BollingerUpper,
                    BollingerLower = point.BollingerLower,
                    TrendScore = point.TrendScore,
                    ConfidenceScore = point.ConfidenceScore,
                    Verdict = point.Verdict,
                    Timestamp = point.Timestamp
                });

                await uow.CompleteAsync();
                return point;
            }
        }

        private static readonly MarketDataTimeframe[] SupportedTimeframes =
        {
            MarketDataTimeframe.OneMinute,
            MarketDataTimeframe.FiveMinutes,
            MarketDataTimeframe.FifteenMinutes,
            MarketDataTimeframe.OneHour,
            MarketDataTimeframe.FourHours
        };

        private async Task UpsertTimeframeCandleAsync(MarketStreamTick tick, MarketDataTimeframe timeframe)
        {
            var bucketOpenTime = timeframe.FloorTimestamp(tick.Timestamp);
            await _marketDataTimeframeCandleRepository.UpsertAsync(
                tick.TenantId,
                tick.Provider,
                tick.AssetClass,
                tick.Symbol,
                timeframe,
                bucketOpenTime,
                tick.Price,
                tick.Timestamp);
        }
    }
}
