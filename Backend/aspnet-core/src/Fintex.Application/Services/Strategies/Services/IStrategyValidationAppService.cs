using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Strategies.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// Validates user-defined strategies and exposes recent validation history.
    /// </summary>
    public interface IStrategyValidationAppService : IApplicationService
    {
        Task<StrategyValidationResultDto> ValidateMyStrategyAsync(ValidateStrategyInput input);

        Task<ListResultDto<StrategyValidationResultDto>> GetMyHistoryAsync(int take = 8);
    }
}
