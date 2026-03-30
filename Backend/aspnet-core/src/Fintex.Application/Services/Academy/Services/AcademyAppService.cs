using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Fintex.Investments;
using Fintex.Investments.Academy.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Investments.Academy
{
    [AbpAuthorize]
    public class AcademyAppService : FintexAppServiceBase, IAcademyAppService
    {
        private readonly IAcademyProgressService _academyProgressService;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IRepository<AcademyQuizAttempt, long> _academyQuizAttemptRepository;

        public AcademyAppService(
            IAcademyProgressService academyProgressService,
            IUserProfileRepository userProfileRepository,
            IRepository<AcademyQuizAttempt, long> academyQuizAttemptRepository)
        {
            _academyProgressService = academyProgressService;
            _userProfileRepository = userProfileRepository;
            _academyQuizAttemptRepository = academyQuizAttemptRepository;
        }

        public Task<AcademyCourseDto> GetIntroCourseAsync()
        {
            return Task.FromResult(AcademyContent.BuildCourse());
        }

        public async Task<AcademyStatusDto> GetMyStatusAsync()
        {
            var status = await _academyProgressService.GetStatusAsync(AbpSession.GetUserId(), AbpSession.TenantId);
            return status.ToDto();
        }

        public async Task<AcademyStatusDto> SaveIntroLessonProgressAsync(SaveAcademyLessonProgressInput input)
        {
            var userId = AbpSession.GetUserId();
            var profile = await _userProfileRepository.GetByUserIdAsync(userId) ?? new UserProfile(AbpSession.TenantId, userId, "USD");
            if (profile.Id == 0)
            {
                await _userProfileRepository.InsertAsync(profile);
            }

            var validLessonKeys = AcademyContent.BuildCourse().Lessons
                .Select(lesson => lesson.Key)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            var currentLessonKey = validLessonKeys.Contains(input?.CurrentLessonKey ?? string.Empty)
                ? input.CurrentLessonKey
                : null;
            var completedLessonKeys = (input?.CompletedLessonKeys ?? new List<string>())
                .Where(key => validLessonKeys.Contains(key ?? string.Empty))
                .ToList();

            profile.UpdateIntroLessonProgress(currentLessonKey, completedLessonKeys);
            await _userProfileRepository.UpdateAsync(profile);
            await CurrentUnitOfWork.SaveChangesAsync();

            var status = await _academyProgressService.GetStatusAsync(userId, AbpSession.TenantId);
            return status.ToDto();
        }

        public async Task<AcademyQuizSubmissionResultDto> SubmitIntroQuizAsync(SubmitAcademyQuizInput input)
        {
            var answers = (input?.Answers ?? new List<AcademyQuizAnswerInput>())
                .Where(item => !string.IsNullOrWhiteSpace(item.QuestionKey))
                .GroupBy(item => item.QuestionKey.Trim().ToLowerInvariant())
                .ToDictionary(group => group.Key, group => group.Last().SelectedOptionKey?.Trim().ToLowerInvariant());

            var answerKey = AcademyContent.AnswerKey;
            var correctAnswers = answerKey.Count(pair =>
                answers.TryGetValue(pair.Key, out var selectedOption) &&
                selectedOption == pair.Value);
            var totalQuestions = answerKey.Count;
            var scorePercent = totalQuestions == 0
                ? 0m
                : decimal.Round((decimal)correctAnswers / totalQuestions * 100m, 2, System.MidpointRounding.AwayFromZero);
            var passed = scorePercent >= AcademyContent.RequiredScorePercent;

            var userId = AbpSession.GetUserId();
            var profile = await _userProfileRepository.GetByUserIdAsync(userId) ?? new UserProfile(AbpSession.TenantId, userId, "USD");
            if (profile.Id == 0)
            {
                await _userProfileRepository.InsertAsync(profile);
            }

            profile.RecordIntroQuizAttempt(scorePercent, AcademyContent.RequiredScorePercent, System.DateTime.UtcNow);
            await _userProfileRepository.UpdateAsync(profile);

            await _academyQuizAttemptRepository.InsertAsync(new AcademyQuizAttempt(
                AbpSession.TenantId,
                userId,
                AcademyContent.IntroCourseKey,
                correctAnswers,
                totalQuestions,
                scorePercent,
                AcademyContent.RequiredScorePercent,
                passed,
                JsonSerializer.Serialize(answers)));

            await CurrentUnitOfWork.SaveChangesAsync();

            var status = await _academyProgressService.GetStatusAsync(userId, AbpSession.TenantId);
            return new AcademyQuizSubmissionResultDto
            {
                Passed = passed,
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions,
                ScorePercent = scorePercent,
                RequiredScorePercent = AcademyContent.RequiredScorePercent,
                Headline = passed ? "Trade academy unlocked" : "Quiz retake required",
                Summary = passed
                    ? "You scored high enough to enter trade academy. Keep building skill on the paper broker until you unlock live broker connectivity."
                    : "You need at least 90% to enter trade academy. Review the lessons and retake the quiz.",
                Status = status.ToDto()
            };
        }
    }
}
