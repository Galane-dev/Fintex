using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    public interface IRecommendationSnapshotCache
    {
        Task<PaperTradeRecommendationDto> GetOrCreateAsync(string symbol, MarketDataProvider provider, Func<Task<PaperTradeRecommendationDto>> factory);
    }
}
