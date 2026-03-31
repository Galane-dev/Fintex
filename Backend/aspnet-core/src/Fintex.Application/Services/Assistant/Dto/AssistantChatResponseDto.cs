using System.Collections.Generic;

namespace Fintex.Investments.Assistant.Dto
{
    /// <summary>
    /// Assistant reply plus any actions that were executed or suggested.
    /// </summary>
    public class AssistantChatResponseDto
    {
        public string Reply { get; set; }

        public string VoiceReply { get; set; }

        public bool UsedAi { get; set; }

        public string Provider { get; set; }

        public string Model { get; set; }

        public List<string> SuggestedPrompts { get; set; } = new List<string>();

        public List<AssistantActionResultDto> ActionResults { get; set; } = new List<AssistantActionResultDto>();
    }
}
