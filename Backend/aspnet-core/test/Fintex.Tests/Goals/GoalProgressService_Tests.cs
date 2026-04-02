using Fintex.Investments.Goals;
using Fintex.Investments.Goals.Services;
using Shouldly;
using System;
using Xunit;

namespace Fintex.Tests.Goals;

public class GoalProgressService_Tests
{
    private readonly GoalProgressService _service = new();

    [Fact]
    public void Should_Mark_Goal_As_Completed_When_Current_Equity_Reaches_Target()
    {
        var now = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc);
        var goal = CreateGoal(deadlineUtc: now.AddDays(2), startEquity: 1000m, targetEquity: 1200m);

        var snapshot = _service.Calculate(goal, currentEquity: 1200m, now);

        snapshot.IsCompleted.ShouldBeTrue();
        snapshot.IsExpired.ShouldBeFalse();
        snapshot.ProgressPercent.ShouldBe(100m);
        snapshot.RequiredDailyGrowthPercent.ShouldBe(0m);
        snapshot.Summary.ShouldContain("target equity has been reached");
    }

    [Fact]
    public void Should_Mark_Goal_As_Expired_When_Deadline_Has_Passed()
    {
        var now = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc);
        var goal = CreateGoal(deadlineUtc: now.AddHours(-1), startEquity: 1000m, targetEquity: 1200m);

        var snapshot = _service.Calculate(goal, currentEquity: 1100m, now);

        snapshot.IsCompleted.ShouldBeFalse();
        snapshot.IsExpired.ShouldBeTrue();
        snapshot.Summary.ShouldContain("deadline passed");
    }

    [Fact]
    public void Should_Calculate_Progress_And_Required_Daily_Growth_For_InFlight_Goal()
    {
        var now = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc);
        var goal = CreateGoal(deadlineUtc: now.AddDays(5), startEquity: 1000m, targetEquity: 1200m);

        var snapshot = _service.Calculate(goal, currentEquity: 1100m, now);

        snapshot.IsCompleted.ShouldBeFalse();
        snapshot.IsExpired.ShouldBeFalse();
        snapshot.ProgressPercent.ShouldBe(50m);
        snapshot.RequiredDailyGrowthPercent.ShouldBe(1.8182m);
        snapshot.Summary.ShouldContain("Progress is 50");
    }

    private static GoalTarget CreateGoal(DateTime deadlineUtc, decimal startEquity, decimal targetEquity)
    {
        return new GoalTarget(
            tenantId: 1,
            userId: 42,
            name: "Goal",
            accountType: GoalAccountType.PaperTrading,
            externalConnectionId: null,
            marketSymbol: "BTCUSDT",
            allowedSymbols: "BTCUSDT",
            targetType: GoalTargetType.PercentGrowth,
            startEquity: startEquity,
            targetEquity: targetEquity,
            targetPercent: 20m,
            deadlineUtc: deadlineUtc,
            maxAcceptableRisk: 30m,
            maxDrawdownPercent: 20m,
            maxPositionSizePercent: 10m,
            tradingSession: GoalTradingSession.AnyTime,
            allowOvernightPositions: true);
    }
}
