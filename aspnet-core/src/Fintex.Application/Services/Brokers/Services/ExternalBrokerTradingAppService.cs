using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Fintex.Investments.Events;
using Fintex.Investments.MarketData;

namespace Fintex.Investments.Brokers
{
    /// <summary>
    /// Routes live trade execution through connected external broker accounts.
    /// </summary>
    [AbpAuthorize]
    public partial class ExternalBrokerTradingAppService : FintexAppServiceBase, IExternalBrokerTradingAppService
    {
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;
        private readonly IAlpacaBrokerService _alpacaBrokerService;
        private readonly IMarketDataPointRepository _marketDataPointRepository;
        private readonly IMarketDataAppService _marketDataAppService;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ITradeRepository _tradeRepository;
        private readonly IRepository<TradeExecutionContext, long> _tradeExecutionContextRepository;
        private readonly IEventBus _eventBus;

        public ExternalBrokerTradingAppService(
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository,
            IAlpacaBrokerService alpacaBrokerService,
            IMarketDataPointRepository marketDataPointRepository,
            IMarketDataAppService marketDataAppService,
            IUserProfileRepository userProfileRepository,
            ITradeRepository tradeRepository,
            IRepository<TradeExecutionContext, long> tradeExecutionContextRepository,
            IEventBus eventBus)
        {
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
            _alpacaBrokerService = alpacaBrokerService;
            _marketDataPointRepository = marketDataPointRepository;
            _marketDataAppService = marketDataAppService;
            _userProfileRepository = userProfileRepository;
            _tradeRepository = tradeRepository;
            _tradeExecutionContextRepository = tradeExecutionContextRepository;
            _eventBus = eventBus;
        }
    }
}
