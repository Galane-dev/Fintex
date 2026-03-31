using System.Collections.Generic;

namespace Fintex.Investments.Academy.Dto
{
    public class SaveAcademyLessonProgressInput
    {
        public string CurrentLessonKey { get; set; }

        public List<string> CompletedLessonKeys { get; set; } = new();
    }
}
