using Abp.Authorization;
using Fintex.Investments.Analytics;

namespace Fintex.Investments.MarketData
{
    /// <summary>
    /// Exposes persisted market data and derived indicator reads to API clients.
    /// </summary>
    [AbpAuthorize]
    public partial class MarketDataAppService : FintexAppServiceBase, IMarketDataAppService
    {
        private const int RsiPeriod = 14;
        private const int RsiWarmupCandles = 30;
        private const int VerdictBarTake = 120;
        private const int ProjectionSmaPeriod = 20;
        private const int ProjectionEmaPeriod = 9;
        private const int ProjectionSmmaPeriod = 14;

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
    }
}
