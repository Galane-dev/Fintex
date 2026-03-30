using System.Collections.Generic;

namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// Request model for strategy validation.
    /// </summary>
    public class ValidateStrategyRequest
    {
        public string StrategyName { get; set; }

        public string Symbol { get; set; }

        public string Timeframe { get; set; }

        public string DirectionPreference { get; set; }

        public string StrategyText { get; set; }
    }

    /// <summary>
    /// Structured AI output for a validated strategy.
    /// </summary>
    public class StrategyValidationInsight
    {
        public decimal ValidationScore { get; set; }

        public StrategyValidationOutcome Outcome { get; set; }

        public string Summary { get; set; }

        public List<string> Strengths { get; set; } = new List<string>();

        public List<string> Risks { get; set; } = new List<string>();

        public List<string> Improvements { get; set; } = new List<string>();

        public string SuggestedAction { get; set; }

        public decimal? SuggestedEntryPrice { get; set; }

        public decimal? SuggestedStopLoss { get; set; }

        public decimal? SuggestedTakeProfit { get; set; }

        public string Provider { get; set; }

        public string Model { get; set; }

        public bool WasGenerated { get; set; }
    }
}
