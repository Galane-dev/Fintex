namespace Fintex.Investments.Trading.Dto
{
    /// <summary>
    /// Coaching summary returned for a closed trade.
    /// </summary>
    public class ClosedTradeReviewDto
    {
        public string Good { get; set; }

        public string Bad { get; set; }

        public string RepeatedPattern { get; set; }

        public string Provider { get; set; }

        public string Model { get; set; }

        public bool WasGenerated { get; set; }
    }
}
