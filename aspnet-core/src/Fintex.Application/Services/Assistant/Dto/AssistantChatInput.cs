using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Assistant.Dto
{
    /// <summary>
    /// Request payload for a new assistant turn.
    /// </summary>
    public class AssistantChatInput
    {
        [Required]
        [StringLength(4000)]
        public string Message { get; set; }

        public bool VoiceMode { get; set; }

        public List<AssistantChatMessageDto> Conversation { get; set; } = new List<AssistantChatMessageDto>();
    }
}
