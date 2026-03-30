namespace Fintex.Investments.Goals.Dto
{
    public class GoalExecutionPlanDto
    {
        public long Id { get; set; }
        public string ExecutionSymbol { get; set; }
        public string SuggestedDirection { get; set; }
        public decimal? SuggestedQuantity { get; set; }
        public decimal? SuggestedStopLoss { get; set; }
        public decimal? SuggestedTakeProfit { get; set; }
        public decimal? RiskScore { get; set; }
        public string Summary { get; set; }
        public string NextAction { get; set; }
        public string GeneratedAtUtc { get; set; }
    }
}
