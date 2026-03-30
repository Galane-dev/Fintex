using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Fintex.Tests.MarketData;

public class MarketProjectionBuilder_Tests
{
    private readonly MarketProjectionBuilder _builder = new();

    [Fact]
    public void Should_Build_A_Mature_Projection_When_Sufficient_Bars_Are_Available()
    {
        var closes = Enumerable.Range(1, 40)
            .Select(index => 60000m + (index * 12m))
            .ToList();
        var referenceTime = new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc);

        var projection = _builder.Build(
            closes,
            currentPrice: 60495m,
            referenceTimestamp: referenceTime,
            minutesAhead: 5,
            modelName: "5m-moving-average-drift",
            smaPeriod: 20,
            emaPeriod: 9,
            smmaPeriod: 14,
            atrPercent: 0.22m);

        projection.Horizon.ShouldBe("5m");
        projection.TargetTimestamp.ShouldBe(referenceTime.AddMinutes(5));
        projection.ModelName.ShouldBe("5m-moving-average-drift");
        projection.ConsensusPrice.ShouldNotBeNull();
        projection.ConfidenceScore.ShouldNotBeNull();
        projection.ConfidenceScore.Value.ShouldBeGreaterThan(50m);
        projection.Maturity.ShouldBe(MarketProjectionMaturity.Mature);
        projection.BarsUsed.ShouldBe(40);
        projection.EffectivePeriod.ShouldBe(20);
    }

    [Fact]
    public void Should_Report_Warming_Up_When_Only_A_Short_Series_Is_Available()
    {
        var closes = new[] { 60000m, 60010m, 60020m, 60030m, 60040m, 60050m }.ToList();

        var projection = _builder.Build(
            closes,
            currentPrice: 60050m,
            referenceTimestamp: DateTime.UtcNow,
            minutesAhead: 1,
            modelName: "1m-moving-average-drift",
            smaPeriod: 20,
            emaPeriod: 9,
            smmaPeriod: 14,
            atrPercent: 0.80m);

        projection.Maturity.ShouldBe(MarketProjectionMaturity.WarmingUp);
        projection.EffectivePeriod.ShouldBe(5);
        projection.ConfidenceScore.ShouldNotBeNull();
        projection.ConfidenceScore.Value.ShouldBeLessThan(50m);
    }
}
