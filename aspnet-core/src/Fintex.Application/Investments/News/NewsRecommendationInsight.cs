using Fintex.Investments.MarketData;
using System;
using System.Collections.Generic;

namespace Fintex.Investments.News
{
    public class NewsRecommendationInsight
    {
        public string FocusKey { get; set; }

        public NewsImpactSentiment Sentiment { get; set; }

        public decimal ImpactScore { get; set; }

        public MarketVerdict RecommendedAction { get; set; }

        public string Summary { get; set; }

        public List<string> KeyHeadlines { get; set; } = new List<string>();

        public string Provider { get; set; }

        public string Model { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime? LatestArticlePublishedAt { get; set; }

        public bool WasGenerated { get; set; }

        public string RawPayloadJson { get; set; }
    }
}
