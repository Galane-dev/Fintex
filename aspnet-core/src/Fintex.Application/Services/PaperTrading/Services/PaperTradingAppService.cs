using Abp.Authorization;
using Fintex.Investments.MarketData;
using Fintex.Investments.News;

namespace Fintex.Investments.PaperTrading
{
    /// <summary>
    /// Orchestrates paper-trading account, recommendation, and execution workflows.
    /// </summary>
    [AbpAuthorize]
    public partial class PaperTradingAppService : FintexAppServiceBase, IPaperTradingAppService
    {
        private readonly IPaperTradingAccountRepository _paperTradingAccountRepository;
        private readonly IPaperOrderRepository _paperOrderRepository;
        private readonly IPaperPositionRepository _paperPositionRepository;
        private readonly IPaperTradeFillRepository _paperTradeFillRepository;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IMarketDataAppService _marketDataAppService;
        private readonly INewsRecommendationService _newsRecommendationService;

        public PaperTradingAppService(
            IPaperTradingAccountRepository paperTradingAccountRepository,
            IPaperOrderRepository paperOrderRepository,
            IPaperPositionRepository paperPositionRepository,
            IPaperTradeFillRepository paperTradeFillRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataAppService marketDataAppService,
            INewsRecommendationService newsRecommendationService)
        {
            _paperTradingAccountRepository = paperTradingAccountRepository;
            _paperOrderRepository = paperOrderRepository;
            _paperPositionRepository = paperPositionRepository;
            _paperTradeFillRepository = paperTradeFillRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataAppService = marketDataAppService;
            _newsRecommendationService = newsRecommendationService;
        }
    }
}
