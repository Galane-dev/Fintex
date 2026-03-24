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
        private readonly ITradeRepository _tradeRepository;
        private readonly IIndicatorCalculator _indicatorCalculator;
        private readonly TradeAnalysisService _tradeAnalysisService;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MarketDataIngestionService(
            IMarketDataPointRepository marketDataPointRepository,
            ITradeRepository tradeRepository,
            IIndicatorCalculator indicatorCalculator,
            TradeAnalysisService tradeAnalysisService,
            IEventBus eventBus,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _marketDataPointRepository = marketDataPointRepository;
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
                var recentPoints = await _marketDataPointRepository.GetRecentAsync(tick.Symbol, 60);
                recentPoints.Reverse();

                var priceSeries = new List<decimal>(recentPoints.Select(x => x.Price)) { tick.Price };
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

                point.ApplyIndicators(indicators.Sma, indicators.Ema, indicators.Rsi, indicators.StdDev);

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
                    Timestamp = point.Timestamp
                });

                await uow.CompleteAsync();
                return point;
            }
        }
    }
}
