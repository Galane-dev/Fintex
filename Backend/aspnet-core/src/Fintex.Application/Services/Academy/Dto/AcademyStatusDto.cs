using System.Collections.Generic;
using System;

namespace Fintex.Investments.Academy.Dto
{
    public class AcademyStatusDto
    {
        public AcademyStage AcademyStage { get; set; }

        public bool HasTradeAcademyAccess { get; set; }

        public bool CanConnectExternalBrokers { get; set; }

        public int IntroQuizAttemptsCount { get; set; }

        public decimal BestIntroQuizScore { get; set; }

        public decimal RequiredQuizScorePercent { get; set; }

        public DateTime? IntroQuizPassedAt { get; set; }

        public DateTime? AcademyGraduatedAt { get; set; }

        public decimal? PaperStartingBalance { get; set; }

        public decimal? PaperEquity { get; set; }

        public decimal PaperGrowthPercent { get; set; }

        public decimal GrowthTargetPercent { get; set; }

        public string CurrentLessonKey { get; set; }

        public List<string> CompletedLessonKeys { get; set; } = new();
    }
}
