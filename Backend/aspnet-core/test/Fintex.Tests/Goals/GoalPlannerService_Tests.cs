using Fintex.Investments;
using Fintex.Investments.Goals;
using Fintex.Investments.Goals.Services;
using Fintex.Investments.PaperTrading;
using Fintex.Investments.PaperTrading.Dto;
using Shouldly;
using System;
using Xunit;

namespace Fintex.Tests.Goals;

public class GoalPlannerService_Tests
{
    private readonly GoalPlannerService _service = new();

    [Fact]
    public void Should_Return_Waiting_Plan_When_Position_Is_Already_Open()
    {
        var goal = CreateGoal(GoalAccountType.PaperTrading, maxPositionSizePercent: 10m);
        var progress = new GoalProgressSnapshot
        {
            CurrentEquity = 1000m,
            RequiredDailyGrowthPercent = 0.2m
        };
        var recommendation = CreateRecommendation(MarketVerdict.Buy, 50000m, 22m, PaperTradeRiskLevel.Low);

        var result = _service.BuildPlan(goal, progress, recommendation, hasOpenExposure: true);

        result.ShouldExecute.ShouldBeFalse();
        result.SuggestedDirection.ShouldBeNull();
        result.Summary.ShouldContain("already open");
        result.NextAction.ShouldContain("Keep monitoring BTC");
    }

    [Fact]
    public void Should_Build_Executable_Buy_Plan_For_Paper_Trading()
    {
        var goal = CreateGoal(GoalAccountType.PaperTrading, maxPositionSizePercent: 10m);
        var progress = new GoalProgressSnapshot
        {
            CurrentEquity = 1000m,
            RequiredDailyGrowthPercent = 0.1m
        };
        var recommendation = CreateRecommendation(MarketVerdict.Buy, 50000m, 18m, PaperTradeRiskLevel.Low);

        var result = _service.BuildPlan(goal, progress, recommendation, hasOpenExposure: false);

        result.ShouldExecute.ShouldBeTrue();
        result.ExecutionSymbol.ShouldBe("BTCUSDT");
        result.SuggestedDirection.ShouldBe(TradeDirection.Buy);
        result.SuggestedQuantity.ShouldBe(0.002m);
        result.SuggestedStopLoss.ShouldBe(49500m);
        result.SuggestedTakeProfit.ShouldBe(51000m);
    }

    [Fact]
    public void Should_Build_Executable_Sell_Plan_For_External_Broker_With_Broker_Symbol()
    {
        var goal = CreateGoal(GoalAccountType.ExternalBroker, maxPositionSizePercent: 15m);
        var progress = new GoalProgressSnapshot
        {
            CurrentEquity = 2000m,
            RequiredDailyGrowthPercent = 0.3m
        };
        var recommendation = CreateRecommendation(MarketVerdict.Sell, 40000m, 20m, PaperTradeRiskLevel.Medium);

        var result = _service.BuildPlan(goal, progress, recommendation, hasOpenExposure: false);

        result.ShouldExecute.ShouldBeTrue();
        result.ExecutionSymbol.ShouldBe("BTCUSD");
        result.SuggestedDirection.ShouldBe(TradeDirection.Sell);
        result.SuggestedQuantity.ShouldBe(0.0075m);
    }

    private static GoalTarget CreateGoal(GoalAccountType accountType, decimal maxPositionSizePercent)
    {
        return new GoalTarget(
            tenantId: 1,
            userId: 42,
            name: "Growth Goal",
            accountType: accountType,
            externalConnectionId: accountType == GoalAccountType.ExternalBroker ? 10 : null,
            marketSymbol: "BTCUSDT",
            allowedSymbols: "BTCUSDT",
            targetType: GoalTargetType.PercentGrowth,
            startEquity: 1000m,
            targetEquity: 1200m,
            targetPercent: 20m,
            deadlineUtc: DateTime.UtcNow.AddDays(2),
            maxAcceptableRisk: 40m,
            maxDrawdownPercent: 20m,
            maxPositionSizePercent: maxPositionSizePercent,
            tradingSession: GoalTradingSession.AnyTime,
            allowOvernightPositions: true);
    }

    private static PaperTradeRecommendationDto CreateRecommendation(
        MarketVerdict action,
        decimal referencePrice,
        decimal riskScore,
        PaperTradeRiskLevel riskLevel)
    {
        return new PaperTradeRecommendationDto
        {
            RecommendedAction = action,
            ReferencePrice = referencePrice,
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            SuggestedStopLoss = referencePrice * 0.99m,
            SuggestedTakeProfit = referencePrice * 1.02m
        };
    }
}
