using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Assistant.Dto
{
    /// <summary>
    /// One chat message exchanged between the user and the Fintex assistant.
    /// </summary>
    public class AssistantChatMessageDto
    {
        [Required]
        [StringLength(16)]
        public string Role { get; set; }

        [Required]
        [StringLength(4000)]
        public string Content { get; set; }
    }
}
