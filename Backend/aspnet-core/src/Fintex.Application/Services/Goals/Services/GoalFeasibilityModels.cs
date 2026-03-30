using System;

namespace Fintex.Investments.Goals.Services
{
    public class GoalFeasibilityRequest
    {
        public GoalTargetType TargetType { get; set; }
        public decimal CurrentEquity { get; set; }
        public decimal? TargetPercent { get; set; }
        public decimal? TargetAmount { get; set; }
        public DateTime DeadlineUtc { get; set; }
        public decimal MaxAcceptableRisk { get; set; }
    }

    public class GoalFeasibilityResult
    {
        public bool IsAccepted { get; set; }
        public decimal TargetEquity { get; set; }
        public decimal TargetPercent { get; set; }
        public decimal RequiredGrowthPercent { get; set; }
        public decimal RequiredDailyGrowthPercent { get; set; }
        public decimal FeasibilityScore { get; set; }
        public string Summary { get; set; }
        public decimal? CounterProposalTargetEquity { get; set; }
        public decimal? CounterProposalTargetPercent { get; set; }
    }

    public class GoalProgressSnapshot
    {
        public decimal CurrentEquity { get; set; }
        public decimal ProgressPercent { get; set; }
        public decimal RequiredDailyGrowthPercent { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsExpired { get; set; }
        public string Summary { get; set; }
    }

    public class GoalPlanDraft
    {
        public string ExecutionSymbol { get; set; }
        public TradeDirection? SuggestedDirection { get; set; }
        public decimal? SuggestedQuantity { get; set; }
        public decimal? SuggestedStopLoss { get; set; }
        public decimal? SuggestedTakeProfit { get; set; }
        public decimal? RiskScore { get; set; }
        public string Summary { get; set; }
        public string NextAction { get; set; }
        public bool ShouldExecute { get; set; }
    }

    public class GoalExecutionResult
    {
        public bool WasExecuted { get; set; }
        public long? TradeId { get; set; }
        public string Summary { get; set; }
        public string Error { get; set; }
    }
}
