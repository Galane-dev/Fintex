using Fintex.Investments;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.PaperTrading;
using Shouldly;
using Xunit;

namespace Fintex.Tests.PaperTrading;

public class RecommendationGuardService_Tests
{
    private readonly RecommendationGuardService _service = new();

    [Fact]
    public void Should_Hold_When_Verdict_Is_Not_Live()
    {
        var shouldHold = _service.ShouldHold(new MarketVerdictDto
        {
            Verdict = MarketVerdict.Buy,
            VerdictState = MarketVerdictState.Degraded,
            ConfidenceScore = 70m,
            TrendScore = 35m
        });

        shouldHold.ShouldBeTrue();
    }

    [Fact]
    public void Should_Hold_When_Confidence_Or_Trend_Is_Weak()
    {
        _service.ShouldHold(new MarketVerdictDto
        {
            Verdict = MarketVerdict.Buy,
            VerdictState = MarketVerdictState.Live,
            ConfidenceScore = 44.9m,
            TrendScore = 35m
        }).ShouldBeTrue();

        _service.ShouldHold(new MarketVerdictDto
        {
            Verdict = MarketVerdict.Sell,
            VerdictState = MarketVerdictState.Live,
            ConfidenceScore = 60m,
            TrendScore = 14.9m
        }).ShouldBeTrue();
    }

    [Fact]
    public void Should_Allow_A_Strong_Live_Verdict()
    {
        var shouldHold = _service.ShouldHold(new MarketVerdictDto
        {
            Verdict = MarketVerdict.Buy,
            VerdictState = MarketVerdictState.Live,
            ConfidenceScore = 66m,
            TrendScore = 28m
        });

        shouldHold.ShouldBeFalse();
    }
}
