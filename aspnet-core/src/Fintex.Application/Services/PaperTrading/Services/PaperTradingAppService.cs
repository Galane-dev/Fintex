using Abp.Authorization;
using Fintex.Investments.Academy;
using Fintex.Investments.EconomicCalendar;
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
        private readonly IEconomicCalendarService _economicCalendarService;
        private readonly IAcademyProgressService _academyProgressService;
        private readonly IRecommendationGuardService _recommendationGuardService;

        public PaperTradingAppService(
            IPaperTradingAccountRepository paperTradingAccountRepository,
            IPaperOrderRepository paperOrderRepository,
            IPaperPositionRepository paperPositionRepository,
            IPaperTradeFillRepository paperTradeFillRepository,
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataAppService marketDataAppService,
            INewsRecommendationService newsRecommendationService,
            IEconomicCalendarService economicCalendarService,
            IAcademyProgressService academyProgressService,
            IRecommendationGuardService recommendationGuardService)
        {
            _paperTradingAccountRepository = paperTradingAccountRepository;
            _paperOrderRepository = paperOrderRepository;
            _paperPositionRepository = paperPositionRepository;
            _paperTradeFillRepository = paperTradeFillRepository;
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataAppService = marketDataAppService;
            _newsRecommendationService = newsRecommendationService;
            _economicCalendarService = economicCalendarService;
            _academyProgressService = academyProgressService;
            _recommendationGuardService = recommendationGuardService;
        }
    }
}
