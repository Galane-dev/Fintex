namespace Fintex.Investments.Goals.Services
{
    public interface IGoalFeasibilityService
    {
        GoalFeasibilityResult Evaluate(GoalFeasibilityRequest request);
    }
}
