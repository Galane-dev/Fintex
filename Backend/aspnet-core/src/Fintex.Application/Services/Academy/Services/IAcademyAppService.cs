using System.Threading.Tasks;
using Fintex.Investments.Academy.Dto;

namespace Fintex.Investments.Academy
{
    public interface IAcademyAppService
    {
        Task<AcademyCourseDto> GetIntroCourseAsync();

        Task<AcademyStatusDto> GetMyStatusAsync();

        Task<AcademyStatusDto> SaveIntroLessonProgressAsync(SaveAcademyLessonProgressInput input);

        Task<AcademyQuizSubmissionResultDto> SubmitIntroQuizAsync(SubmitAcademyQuizInput input);
    }
}
