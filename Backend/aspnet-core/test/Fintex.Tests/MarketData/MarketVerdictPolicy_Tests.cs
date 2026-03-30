using Fintex.Investments;
using Fintex.Investments.MarketData;
using Shouldly;
using Xunit;

namespace Fintex.Tests.MarketData;

public class MarketVerdictPolicy_Tests
{
    private readonly MarketVerdictPolicy _policy = new();

    [Fact]
    public void Should_Return_Hold_When_Confidence_Is_Below_Minimum()
    {
        var verdict = _policy.ResolveVerdict(42m, 39.9m);

        verdict.ShouldBe(MarketVerdict.Hold);
    }

    [Fact]
    public void Should_Return_Hold_When_Trend_Is_Too_Small()
    {
        var verdict = _policy.ResolveVerdict(14.9m, 70m);

        verdict.ShouldBe(MarketVerdict.Hold);
    }

    [Fact]
    public void Should_Return_Buy_Or_Sell_For_Strong_Live_Signals()
    {
        _policy.ResolveVerdict(28m, 68m).ShouldBe(MarketVerdict.Buy);
        _policy.ResolveVerdict(-31m, 68m).ShouldBe(MarketVerdict.Sell);
    }
}
