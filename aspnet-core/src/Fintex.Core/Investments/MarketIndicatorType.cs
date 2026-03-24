namespace Fintex.Investments
{
    /// <summary>
    /// Identifies a single persisted market indicator value.
    /// </summary>
    public enum MarketIndicatorType
    {
        Sma = 1,
        Ema = 2,
        Rsi = 3,
        StdDev = 4,
        Macd = 5,
        MacdSignal = 6,
        MacdHistogram = 7,
        Momentum = 8,
        RateOfChange = 9,
        BollingerUpper = 10,
        BollingerLower = 11,
        TrendScore = 12,
        ConfidenceScore = 13
    }
}
