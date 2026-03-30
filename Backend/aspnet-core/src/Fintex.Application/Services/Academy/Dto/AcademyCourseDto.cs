using System.Collections.Generic;

namespace Fintex.Investments.Academy.Dto
{
    public class AcademyCourseDto
    {
        public string Key { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public decimal RequiredScorePercent { get; set; }

        public List<AcademyLessonDto> Lessons { get; set; } = new();

        public List<AcademyQuizQuestionDto> QuizQuestions { get; set; } = new();
    }
}
