namespace Fintex.Investments.MarketData
{
    public interface IMarketVerdictPolicy
    {
        MarketVerdict ResolveVerdict(decimal trendScore, decimal confidenceScore);
    }
}
