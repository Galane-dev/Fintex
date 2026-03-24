using Abp.Application.Services.Dto;
using Abp.Authorization;
using Fintex.Investments.MarketData.Dto;
using Microsoft.EntityFrameworkCore;
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
        private readonly IMarketDataPointRepository _marketDataPointRepository;

        public MarketDataAppService(IMarketDataPointRepository marketDataPointRepository)
        {
            _marketDataPointRepository = marketDataPointRepository;
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
    }
}
