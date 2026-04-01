using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Assistant.Dto
{
    /// <summary>
    /// Minimal client context used when minting a browser-safe Realtime voice session.
    /// </summary>
    public class AssistantRealtimeSessionInput
    {
        [StringLength(128)]
        public string ClientTimeZone { get; set; }

        [StringLength(64)]
        public string ClientNowIso { get; set; }
    }
}
