namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Result of a paper trade placement attempt, including whether execution was allowed.
    /// </summary>
    public class PaperTradeExecutionResultDto
    {
        public bool WasExecuted { get; set; }

        public PaperTradeAssessmentDto Assessment { get; set; }

        public PaperOrderDto Order { get; set; }
    }
}
