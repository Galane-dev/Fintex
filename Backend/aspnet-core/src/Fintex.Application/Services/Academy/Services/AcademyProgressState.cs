using Fintex.Investments.Academy.Dto;
using Fintex.Investments;
using System;
using System.Linq;

namespace Fintex.Investments.Academy
{
    /// <summary>
    /// Internal academy status snapshot used for gating and status responses.
    /// </summary>
    public class AcademyProgressState
    {
        public UserProfile Profile { get; set; }

        public PaperTradingAccount PaperAccount { get; set; }

        public decimal PaperGrowthPercent { get; set; }

        public decimal GrowthTargetPercent { get; set; }

        public AcademyStatusDto ToDto()
        {
            return new AcademyStatusDto
            {
                AcademyStage = Profile.AcademyStage,
                HasTradeAcademyAccess = Profile.IntroQuizPassedAt.HasValue,
                CanConnectExternalBrokers = Profile.AcademyStage == AcademyStage.Graduated,
                IntroQuizAttemptsCount = Profile.IntroQuizAttemptsCount,
                BestIntroQuizScore = Profile.BestIntroQuizScore,
                RequiredQuizScorePercent = AcademyContent.RequiredScorePercent,
                IntroQuizPassedAt = Profile.IntroQuizPassedAt,
                AcademyGraduatedAt = Profile.AcademyGraduatedAt,
                PaperStartingBalance = PaperAccount?.StartingBalance,
                PaperEquity = PaperAccount?.Equity,
                PaperGrowthPercent = PaperGrowthPercent,
                GrowthTargetPercent = GrowthTargetPercent,
                CurrentLessonKey = Profile.CurrentIntroLessonKey,
                CompletedLessonKeys = (Profile.CompletedIntroLessonKeys ?? string.Empty)
                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList()
            };
        }
    }
}
