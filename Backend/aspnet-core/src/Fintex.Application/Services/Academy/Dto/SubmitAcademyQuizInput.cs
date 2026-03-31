using System.Collections.Generic;

namespace Fintex.Investments.Academy.Dto
{
    public class SubmitAcademyQuizInput
    {
        public List<AcademyQuizAnswerInput> Answers { get; set; } = new();
    }
}
