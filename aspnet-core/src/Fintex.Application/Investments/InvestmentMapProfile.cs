using AutoMapper;
using Fintex.Investments.Analytics.Dto;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.Profiles.Dto;
using Fintex.Investments.Trading.Dto;

namespace Fintex.Investments
{
    /// <summary>
    /// AutoMapper profile for the investment domain DTOs.
    /// </summary>
    public class InvestmentMapProfile : Profile
    {
        public InvestmentMapProfile()
        {
            CreateMap<Trade, TradeDto>();
            CreateMap<MarketDataPoint, MarketDataPointDto>();
            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<TradeAnalysisSnapshot, TradeAnalysisSnapshotDto>();
        }
    }
}
