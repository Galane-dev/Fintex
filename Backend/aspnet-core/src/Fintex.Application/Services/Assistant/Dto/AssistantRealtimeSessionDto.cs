namespace Fintex.Investments.Assistant.Dto
{
    /// <summary>
    /// Browser-safe OpenAI Realtime session details. The frontend uses the short-lived client secret
    /// to connect directly to the Realtime API over WebRTC.
    /// </summary>
    public class AssistantRealtimeSessionDto
    {
        public string ClientSecret { get; set; }

        public string ExpiresAtUtc { get; set; }

        public string Model { get; set; }

        public string Voice { get; set; }

        public string Instructions { get; set; }
    }
}
