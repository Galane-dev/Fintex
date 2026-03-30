namespace Fintex.Investments.Goals.Dto
{
    public class GoalExecutionEventDto
    {
        public long Id { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public long? TradeId { get; set; }
        public decimal? EquityAfterExecution { get; set; }
        public string OccurredAtUtc { get; set; }
    }
}
