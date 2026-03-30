using System;

namespace Fintex.Investments.Goals.Services
{
    public interface IGoalProgressService
    {
        GoalProgressSnapshot Calculate(GoalTarget goal, decimal currentEquity, DateTime nowUtc);
    }
}
