using Fintex.Investments.PaperTrading.Dto;

namespace Fintex.Investments.Goals.Services
{
    public interface IGoalPlannerService
    {
        GoalPlanDraft BuildPlan(
            GoalTarget goal,
            GoalProgressSnapshot progress,
            PaperTradeRecommendationDto recommendation,
            bool hasOpenExposure);
    }
}
