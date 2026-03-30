using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Goals.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Goals.Services
{
    public interface IGoalAutomationAppService : IApplicationService
    {
        Task<ListResultDto<GoalTargetDto>> GetMyGoalsAsync();

        Task<GoalTargetDto> CreateGoalAsync(CreateGoalTargetInput input);

        Task<GoalTargetDto> PauseGoalAsync(EntityDto<long> input);

        Task<GoalTargetDto> ResumeGoalAsync(EntityDto<long> input);

        Task<GoalTargetDto> CancelGoalAsync(EntityDto<long> input);
    }
}
