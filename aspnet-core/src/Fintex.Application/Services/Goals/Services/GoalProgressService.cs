using Abp.Dependency;
using System;

namespace Fintex.Investments.Goals.Services
{
    public class GoalProgressService : IGoalProgressService, ITransientDependency
    {
        public GoalProgressSnapshot Calculate(GoalTarget goal, decimal currentEquity, DateTime nowUtc)
        {
            var equityDelta = goal.TargetEquity - goal.StartEquity;
            var achievedDelta = currentEquity - goal.StartEquity;
            var progress = equityDelta <= 0m ? 100m : Clamp(achievedDelta / equityDelta * 100m, 0m, 100m);
            var remainingPercent = currentEquity <= 0m ? 0m : ((goal.TargetEquity - currentEquity) / currentEquity) * 100m;
            var remainingDays = (decimal)(goal.DeadlineUtc - nowUtc).TotalDays;
            var requiredDailyGrowthPercent = remainingPercent <= 0m || remainingDays <= 0m ? 0m : remainingPercent / remainingDays;

            return new GoalProgressSnapshot
            {
                CurrentEquity = decimal.Round(currentEquity, 8, MidpointRounding.AwayFromZero),
                ProgressPercent = decimal.Round(progress, 4, MidpointRounding.AwayFromZero),
                RequiredDailyGrowthPercent = decimal.Round(requiredDailyGrowthPercent, 4, MidpointRounding.AwayFromZero),
                IsCompleted = currentEquity >= goal.TargetEquity,
                IsExpired = nowUtc >= goal.DeadlineUtc,
                Summary = currentEquity >= goal.TargetEquity
                    ? "The target equity has been reached."
                    : nowUtc >= goal.DeadlineUtc
                        ? "The deadline passed before the target equity was reached."
                        : $"Progress is {progress:0.##}% and the account now needs about {requiredDailyGrowthPercent:0.####}% daily growth to stay on pace."
            };
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
