using System.Collections.Generic;

namespace Fintex.Investments.Goals.Dto
{
    public class GoalTargetDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AccountType { get; set; }
        public long? ExternalConnectionId { get; set; }
        public string ExternalConnectionName { get; set; }
        public string MarketSymbol { get; set; }
        public string AllowedSymbols { get; set; }
        public string TargetType { get; set; }
        public decimal StartEquity { get; set; }
        public decimal CurrentEquity { get; set; }
        public decimal TargetEquity { get; set; }
        public decimal TargetPercent { get; set; }
        public string DeadlineUtc { get; set; }
        public decimal MaxAcceptableRisk { get; set; }
        public decimal MaxDrawdownPercent { get; set; }
        public decimal MaxPositionSizePercent { get; set; }
        public string TradingSession { get; set; }
        public bool AllowOvernightPositions { get; set; }
        public string Status { get; set; }
        public string StatusReason { get; set; }
        public decimal ProgressPercent { get; set; }
        public decimal RequiredDailyGrowthPercent { get; set; }
        public string LatestPlanSummary { get; set; }
        public string LatestNextAction { get; set; }
        public string LastEvaluatedAtUtc { get; set; }
        public string LastExecutedAtUtc { get; set; }
        public string LastExecutionAttemptAtUtc { get; set; }
        public int ExecutedTradesCount { get; set; }
        public long? LastTradeId { get; set; }
        public string LastError { get; set; }
        public GoalEvaluationRunDto LatestEvaluation { get; set; }
        public GoalExecutionPlanDto LatestPlan { get; set; }
        public List<GoalExecutionEventDto> Events { get; set; } = new List<GoalExecutionEventDto>();
    }
}
