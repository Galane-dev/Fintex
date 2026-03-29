using System.Collections.Generic;

namespace Fintex.Investments.Academy.Dto
{
    public class AcademyQuizQuestionDto
    {
        public string Key { get; set; }

        public string Prompt { get; set; }

        public List<AcademyQuizOptionDto> Options { get; set; } = new();
    }
}
