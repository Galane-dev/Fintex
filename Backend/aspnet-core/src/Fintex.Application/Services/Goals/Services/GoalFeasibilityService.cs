using Abp.Dependency;
using System;

namespace Fintex.Investments.Goals.Services
{
    public class GoalFeasibilityService : IGoalFeasibilityService, ITransientDependency
    {
        private const decimal BaseDailyGrowthAllowance = 0.35m;

        public GoalFeasibilityResult Evaluate(GoalFeasibilityRequest request)
        {
            var nowUtc = DateTime.UtcNow;
            var days = (decimal)(request.DeadlineUtc - nowUtc).TotalDays;
            var targetEquity = request.TargetType == GoalTargetType.TargetAmount
                ? request.TargetAmount.GetValueOrDefault()
                : request.CurrentEquity * (1m + request.TargetPercent.GetValueOrDefault() / 100m);
            var targetPercent = request.TargetType == GoalTargetType.TargetAmount
                ? ((targetEquity - request.CurrentEquity) / request.CurrentEquity) * 100m
                : request.TargetPercent.GetValueOrDefault();
            var requiredGrowthPercent = targetPercent;
            var requiredDailyGrowthPercent = days <= 0m ? requiredGrowthPercent : requiredGrowthPercent / days;
            var allowedDailyGrowthPercent = BaseDailyGrowthAllowance + (request.MaxAcceptableRisk / 25m);
            var score = decimal.Round(requiredDailyGrowthPercent / allowedDailyGrowthPercent * 100m, 4, MidpointRounding.AwayFromZero);
            var accepted = days >= 1m &&
                days <= 7m &&
                request.CurrentEquity > 0m &&
                targetEquity > request.CurrentEquity &&
                requiredDailyGrowthPercent > 0m &&
                score <= 100m;

            var feasibleDailyPercent = decimal.Round(allowedDailyGrowthPercent, 4, MidpointRounding.AwayFromZero);
            var counterProposalPercent = accepted
                ? (decimal?)null
                : decimal.Round(feasibleDailyPercent * Clamp(days, 1m, 7m), 4, MidpointRounding.AwayFromZero);
            var counterProposalTargetEquity = accepted || !counterProposalPercent.HasValue
                ? (decimal?)null
                : decimal.Round(
                    request.CurrentEquity * (1m + counterProposalPercent.Value / 100m),
                    8,
                    MidpointRounding.AwayFromZero);

            return new GoalFeasibilityResult
            {
                IsAccepted = accepted,
                TargetEquity = decimal.Round(targetEquity, 8, MidpointRounding.AwayFromZero),
                TargetPercent = decimal.Round(targetPercent, 4, MidpointRounding.AwayFromZero),
                RequiredGrowthPercent = decimal.Round(requiredGrowthPercent, 4, MidpointRounding.AwayFromZero),
                RequiredDailyGrowthPercent = decimal.Round(requiredDailyGrowthPercent, 4, MidpointRounding.AwayFromZero),
                FeasibilityScore = Clamp(decimal.Round(score, 4, MidpointRounding.AwayFromZero), 0m, 200m),
                Summary = accepted
                    ? $"Accepted on a best-effort basis. The target needs about {requiredDailyGrowthPercent:0.####}% daily growth over {days:0.##} days."
                    : BuildRejectionSummary(days, requiredDailyGrowthPercent, feasibleDailyPercent, counterProposalPercent),
                CounterProposalTargetEquity = counterProposalTargetEquity,
                CounterProposalTargetPercent = counterProposalPercent
            };
        }

        private static string BuildRejectionSummary(
            decimal days,
            decimal requiredDailyGrowthPercent,
            decimal feasibleDailyPercent,
            decimal? counterProposalPercent)
        {
            if (days < 1m || days > 7m)
            {
                return "Rejected. Goal deadlines must be between 1 and 7 days for this BTC autopilot MVP.";
            }

            if (!counterProposalPercent.HasValue)
            {
                return "Rejected. The requested target is not feasible within the configured risk budget.";
            }

            return $"Rejected. The goal needs about {requiredDailyGrowthPercent:0.####}% daily growth, which is above the current best-effort ceiling of {feasibleDailyPercent:0.####}% daily. Try about {counterProposalPercent.Value:0.####}% total growth instead.";
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
