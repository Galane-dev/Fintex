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

        [StringLength(128)]
        public string ClientTimeZone { get; set; }

        [StringLength(64)]
        public string ClientNowIso { get; set; }

        public List<AssistantChatMessageDto> Conversation { get; set; } = new List<AssistantChatMessageDto>();
    }
}
