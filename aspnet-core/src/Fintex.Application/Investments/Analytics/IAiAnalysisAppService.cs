using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Analytics.Dto;
using Fintex.Investments.Profiles.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Application contract for AI and analytics endpoints.
    /// </summary>
    public interface IAiAnalysisAppService : IApplicationService
    {
        Task<ListResultDto<TradeAnalysisSnapshotDto>> GetTradeSnapshotsAsync(EntityDto<long> input);

        Task<UserProfileDto> RefreshMyBehavioralProfileAsync();
    }
}
