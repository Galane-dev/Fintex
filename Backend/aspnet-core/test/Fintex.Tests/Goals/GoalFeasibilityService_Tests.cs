using Fintex.Investments.Goals;
using Fintex.Investments.Goals.Services;
using Shouldly;
using System;
using Xunit;

namespace Fintex.Tests.Goals;

public class GoalFeasibilityService_Tests
{
    private readonly GoalFeasibilityService _service = new();

    [Fact]
    public void Should_Accept_A_Feasible_Percent_Goal_Within_The_Mvp_Window()
    {
        var result = _service.Evaluate(new GoalFeasibilityRequest
        {
            TargetType = GoalTargetType.PercentGrowth,
            CurrentEquity = 1000m,
            TargetPercent = 0.25m,
            DeadlineUtc = DateTime.UtcNow.AddDays(2),
            MaxAcceptableRisk = 45m
        });

        result.IsAccepted.ShouldBeTrue();
        result.TargetEquity.ShouldBe(1002.5m);
        result.CounterProposalTargetPercent.ShouldBeNull();
        result.Summary.ShouldContain("Accepted on a best-effort basis");
    }

    [Fact]
    public void Should_Reject_An_Overly_Aggressive_Goal_And_Return_A_Counter_Proposal()
    {
        var result = _service.Evaluate(new GoalFeasibilityRequest
        {
            TargetType = GoalTargetType.PercentGrowth,
            CurrentEquity = 1000m,
            TargetPercent = 10m,
            DeadlineUtc = DateTime.UtcNow.AddDays(2),
            MaxAcceptableRisk = 20m
        });

        result.IsAccepted.ShouldBeFalse();
        result.CounterProposalTargetPercent.ShouldNotBeNull();
        result.CounterProposalTargetEquity.ShouldNotBeNull();
        result.Summary.ShouldContain("Rejected.");
        result.Summary.ShouldContain("Try about");
    }

    [Fact]
    public void Should_Reject_Goals_Outside_The_One_To_Seven_Day_Window()
    {
        var result = _service.Evaluate(new GoalFeasibilityRequest
        {
            TargetType = GoalTargetType.TargetAmount,
            CurrentEquity = 1000m,
            TargetAmount = 1010m,
            DeadlineUtc = DateTime.UtcNow.AddDays(8),
            MaxAcceptableRisk = 30m
        });

        result.IsAccepted.ShouldBeFalse();
        result.Summary.ShouldContain("between 1 and 7 days");
    }
}
