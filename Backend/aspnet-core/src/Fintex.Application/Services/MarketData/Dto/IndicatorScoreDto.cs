namespace Fintex.Investments.MarketData.Dto
{
    /// <summary>
    /// DTO for exposing the contribution of a single market indicator.
    /// </summary>
    public class IndicatorScoreDto
    {
        public string Name { get; set; }

        public decimal Value { get; set; }

        public decimal Score { get; set; }

        public IndicatorSignal Signal { get; set; }
    }
}
