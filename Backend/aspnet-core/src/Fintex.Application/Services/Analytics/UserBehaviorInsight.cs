namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Represents the result returned by an external behavioral AI provider.
    /// </summary>
    public class UserBehaviorInsight
    {
        public decimal RiskScore { get; set; }

        public string Summary { get; set; }

        public string Provider { get; set; }

        public string Model { get; set; }

        public bool WasGenerated { get; set; }
    }
}
