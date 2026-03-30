namespace Fintex.Investments.Academy.Dto
{
    public class AcademyQuizSubmissionResultDto
    {
        public bool Passed { get; set; }

        public int CorrectAnswers { get; set; }

        public int TotalQuestions { get; set; }

        public decimal ScorePercent { get; set; }

        public decimal RequiredScorePercent { get; set; }

        public string Headline { get; set; }

        public string Summary { get; set; }

        public AcademyStatusDto Status { get; set; }
    }
}
