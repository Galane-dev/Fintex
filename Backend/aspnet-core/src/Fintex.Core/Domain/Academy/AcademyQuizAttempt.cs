using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments.Academy
{
    /// <summary>
    /// Stores the result of a user's intro academy quiz attempt.
    /// </summary>
    public class AcademyQuizAttempt : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxCourseKeyLength = 64;
        public const int MaxAnswersJsonLength = 4000;

        protected AcademyQuizAttempt()
        {
        }

        public AcademyQuizAttempt(
            int? tenantId,
            long userId,
            string courseKey,
            int correctAnswers,
            int totalQuestions,
            decimal scorePercent,
            decimal requiredScorePercent,
            bool passed,
            string answersJson)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User id must be greater than zero.");
            }

            TenantId = tenantId;
            UserId = userId;
            CourseKey = LimitRequired(courseKey, MaxCourseKeyLength, "Course key is required.");
            CorrectAnswers = correctAnswers < 0 ? 0 : correctAnswers;
            TotalQuestions = totalQuestions <= 0 ? 0 : totalQuestions;
            ScorePercent = Round(scorePercent);
            RequiredScorePercent = Round(requiredScorePercent);
            Passed = passed;
            AnswersJson = LimitOptional(answersJson, MaxAnswersJsonLength);
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string CourseKey { get; protected set; }

        public int CorrectAnswers { get; protected set; }

        public int TotalQuestions { get; protected set; }

        public decimal ScorePercent { get; protected set; }

        public decimal RequiredScorePercent { get; protected set; }

        public bool Passed { get; protected set; }

        public string AnswersJson { get; protected set; }

        private static decimal Round(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static string LimitRequired(string value, int maxLength, string error)
        {
            var limited = LimitOptional(value, maxLength);
            if (string.IsNullOrWhiteSpace(limited))
            {
                throw new ArgumentException(error);
            }

            return limited;
        }

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
