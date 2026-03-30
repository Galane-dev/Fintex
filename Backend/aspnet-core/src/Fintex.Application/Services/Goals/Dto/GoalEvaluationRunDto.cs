namespace Fintex.Investments.Goals.Dto
{
    public class GoalEvaluationRunDto
    {
        public long Id { get; set; }
        public string GoalStatus { get; set; }
        public decimal CurrentEquity { get; set; }
        public decimal RequiredGrowthPercent { get; set; }
        public decimal RequiredDailyGrowthPercent { get; set; }
        public decimal FeasibilityScore { get; set; }
        public string Summary { get; set; }
        public decimal? CounterProposalTargetEquity { get; set; }
        public decimal? CounterProposalTargetPercent { get; set; }
        public string OccurredAtUtc { get; set; }
    }
}
