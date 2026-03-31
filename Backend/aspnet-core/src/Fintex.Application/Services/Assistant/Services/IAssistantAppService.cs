using Abp.Application.Services;
using Fintex.Investments.Assistant.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Assistant
{
    /// <summary>
    /// Conversational assistant for explaining and operating the Fintex dashboard.
    /// </summary>
    public interface IAssistantAppService : IApplicationService
    {
        Task<AssistantChatResponseDto> SendMessageAsync(AssistantChatInput input);
    }
}
