namespace Fintex.Investments.Assistant.Dto
{
    /// <summary>
    /// Describes one action the assistant attempted or suggested.
    /// </summary>
    public class AssistantActionResultDto
    {
        public string ActionType { get; set; }

        public string Status { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }
    }
}
