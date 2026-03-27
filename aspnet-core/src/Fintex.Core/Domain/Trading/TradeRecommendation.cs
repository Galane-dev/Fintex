namespace Fintex.Investments
{
    /// <summary>
    /// Represents the current recommendation produced by analytics.
    /// </summary>
    public enum TradeRecommendation
    {
        Monitor = 1,
        Hold = 2,
        ScaleIn = 3,
        ReduceExposure = 4,
        Exit = 5
    }
}
