using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Fintex.Investments.Automation.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Automation
{
    /// <summary>
    /// Manages the current user's automatic trade execution rules.
    /// </summary>
    public interface ITradeAutomationAppService : IApplicationService
    {
        Task<ListResultDto<TradeAutomationRuleDto>> GetMyRulesAsync();

        Task<TradeAutomationRuleDto> CreateRuleAsync(CreateTradeAutomationRuleInput input);

        Task DeleteRuleAsync(EntityDto<long> input);
    }
}
