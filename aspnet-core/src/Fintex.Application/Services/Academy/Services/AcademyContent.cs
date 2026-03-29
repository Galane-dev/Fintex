using Fintex.Investments.Academy.Dto;
using System.Collections.Generic;

namespace Fintex.Investments.Academy
{
    internal static class AcademyContent
    {
        public const string IntroCourseKey = "fintex-intro-trading";
        public const decimal RequiredScorePercent = 90m;
        public const decimal GraduationGrowthTargetPercent = 75m;

        public static AcademyCourseDto BuildCourse()
        {
            return new AcademyCourseDto
            {
                Key = IntroCourseKey,
                Title = "Fintex Trading Foundations",
                Subtitle = "Learn how markets work, how risk is managed, and how Fintex trains you from simulator to live execution.",
                RequiredScorePercent = RequiredScorePercent,
                Lessons = BuildLessons(),
                QuizQuestions = BuildQuestions()
            };
        }

        internal static IReadOnlyDictionary<string, string> AnswerKey => new Dictionary<string, string>
        {
            ["q1"] = "b",
            ["q2"] = "c",
            ["q3"] = "a",
            ["q4"] = "d",
            ["q5"] = "b",
            ["q6"] = "a",
            ["q7"] = "c",
            ["q8"] = "d",
            ["q9"] = "b",
            ["q10"] = "a"
        };

        private static List<AcademyLessonDto> BuildLessons()
        {
            return new List<AcademyLessonDto>
            {
                new() { Key = "markets", Title = "How trading works", Summary = "Understand price movement, buyers vs sellers, and why risk matters more than excitement.", ContentMarkdown = "- Markets move because buyers and sellers keep re-pricing value.\n- Trading is not guessing; it is taking measured risk when a setup has edge.\n- Every trade needs an entry idea, an invalidation level, and a target or management plan.\n- Professionals survive first, then grow. Capital protection is part of the strategy." },
                new() { Key = "assets", Title = "Crypto, forex, commodities, and stocks", Summary = "Know what you are trading and why each market behaves differently.", ContentMarkdown = "- Crypto trades almost 24/7 and can move sharply on sentiment, liquidity, and macro news.\n- Forex tracks currency strength and is heavily influenced by interest rates and economic releases.\n- Commodities react to supply, demand, geopolitics, and seasonality.\n- Stocks reflect company performance, expectations, and broader market risk appetite." },
                new() { Key = "risk", Title = "Risk and execution basics", Summary = "A good setup is defined by controlled downside and repeatable execution.", ContentMarkdown = "- Stop loss defines what proves the idea wrong.\n- Take profit or management rules define how reward is captured.\n- Position size should fit the risk on the setup, not emotion.\n- A strategy without clear invalidation is not a professional plan." },
                new() { Key = "fintex", Title = "How Fintex works", Summary = "Learn the platform flow from verdicts and recommendations to academy progression.", ContentMarkdown = "- The dashboard combines live technical context, news overlays, alerts, and automation.\n- Trade academy keeps you on the internal paper broker until you prove consistency.\n- External broker linking unlocks only after you pass the intro quiz and grow your academy account by 75%.\n- Strategy validation, behavior analysis, and insights are there to improve discipline, not just prediction." },
                new() { Key = "progression", Title = "Your path to professional trading", Summary = "The system is designed to build competence before access.", ContentMarkdown = "- Step 1: complete the intro course and pass the quiz with at least 90%.\n- Step 2: trade only in the academy simulator while building skill.\n- Step 3: grow the academy account by at least 75% from starting balance.\n- Step 4: unlock external broker connectivity only after proving both knowledge and execution." }
            };
        }

        private static List<AcademyQuizQuestionDto> BuildQuestions()
        {
            return new List<AcademyQuizQuestionDto>
            {
                Question("q1", "What makes a trade professional rather than random?", ("a", "High confidence and no stop"), ("b", "Defined entry, risk, and invalidation"), ("c", "Trading only when the market is moving fast"), ("d", "Using maximum leverage")),
                Question("q2", "Which market is most directly driven by central-bank and macro releases?", ("a", "Commodities"), ("b", "Crypto"), ("c", "Forex"), ("d", "Meme coins")),
                Question("q3", "What is the main purpose of a stop loss?", ("a", "Define what proves the trade idea wrong"), ("b", "Guarantee profit"), ("c", "Increase leverage safely"), ("d", "Replace position sizing")),
                Question("q4", "What must happen before Fintex unlocks external brokers?", ("a", "One profitable trade"), ("b", "Ten alerts created"), ("c", "Behavior analysis refresh"), ("d", "Pass the quiz and grow the paper account by 75%")),
                Question("q5", "Why does Fintex use trade academy first?", ("a", "To block all trading forever"), ("b", "To build skill before live-market access"), ("c", "Because live brokers cannot connect"), ("d", "To avoid showing charts")),
                Question("q6", "What should every trade setup include?", ("a", "Entry, invalidation, and management plan"), ("b", "Only a target"), ("c", "Only a verdict"), ("d", "Only a news headline")),
                Question("q7", "Why can crypto behave differently from stocks?", ("a", "It is always safer"), ("b", "It ignores sentiment"), ("c", "It trades nearly nonstop and reacts sharply to liquidity and sentiment"), ("d", "It has no macro sensitivity")),
                Question("q8", "What does a strategy without clear invalidation become?", ("a", "A hedge"), ("b", "An investment plan"), ("c", "A market-making system"), ("d", "A weak and unprofessional setup")),
                Question("q9", "What score is required to enter trade academy?", ("a", "70%"), ("b", "90%"), ("c", "100%"), ("d", "Any passing score")),
                Question("q10", "What is Fintex trying to turn users into?", ("a", "Professional traders with disciplined process"), ("b", "Signal followers"), ("c", "Only crypto holders"), ("d", "News readers"))
            };
        }

        private static AcademyQuizQuestionDto Question(string key, string prompt, params (string Key, string Label)[] options)
        {
            var question = new AcademyQuizQuestionDto
            {
                Key = key,
                Prompt = prompt
            };

            foreach (var option in options)
            {
                question.Options.Add(new AcademyQuizOptionDto
                {
                    Key = option.Key,
                    Label = option.Label
                });
            }

            return question;
        }
    }
}
