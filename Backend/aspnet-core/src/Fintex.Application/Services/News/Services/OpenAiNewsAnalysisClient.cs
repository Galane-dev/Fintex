using Abp.Dependency;
using Fintex.Investments.MarketData;
using Fintex.Investments.MarketData.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.News
{
    public class OpenAiNewsAnalysisClient : INewsAnalysisClient, ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAiNewsAnalysisClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<NewsRecommendationInsight> AnalyzeAsync(
            string focusKey,
            IReadOnlyList<NewsArticle> articles,
            MarketVerdictDto marketVerdict,
            CancellationToken cancellationToken)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/responses";

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(model) ||
                articles == null ||
                articles.Count == 0)
            {
                return BuildFallbackInsight(focusKey, articles, marketVerdict, model);
            }

            var prompt = BuildPrompt(articles, marketVerdict);
            var payload = JsonSerializer.Serialize(new
            {
                model = model,
                input = prompt
            });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseText = ExtractResponseText(responseContent);
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return BuildFallbackInsight(focusKey, articles, marketVerdict, model);
            }

            try
            {
                using (var json = JsonDocument.Parse(responseText))
                {
                    var headlines = new List<string>();
                    if (json.RootElement.TryGetProperty("keyHeadlines", out var headlineElement) &&
                        headlineElement.ValueKind == JsonValueKind.Array)
                    {
                        headlines.AddRange(
                            headlineElement.EnumerateArray()
                                .Where(x => x.ValueKind == JsonValueKind.String)
                                .Select(x => x.GetString())
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Take(5));
                    }

                    return new NewsRecommendationInsight
                    {
                        FocusKey = focusKey,
                        Sentiment = ParseSentiment(json.RootElement.TryGetProperty("sentiment", out var sentimentElement)
                            ? sentimentElement.GetString()
                            : null),
                        ImpactScore = Clamp(json.RootElement.TryGetProperty("impactScore", out var impactElement)
                            ? impactElement.GetDecimal()
                            : 35m, 0m, 100m),
                        RecommendedAction = ParseAction(json.RootElement.TryGetProperty("recommendedAction", out var actionElement)
                            ? actionElement.GetString()
                            : null),
                        Summary = json.RootElement.TryGetProperty("summary", out var summaryElement)
                            ? summaryElement.GetString()
                            : BuildDefaultSummary(articles, marketVerdict),
                        KeyHeadlines = headlines,
                        Provider = "OpenAI",
                        Model = model,
                        GeneratedAt = DateTime.UtcNow,
                        LatestArticlePublishedAt = articles.Max(x => x.PublishedAt),
                        WasGenerated = true,
                        RawPayloadJson = responseText
                    };
                }
            }
            catch (JsonException)
            {
                return BuildFallbackInsight(focusKey, articles, marketVerdict, model, responseText);
            }
        }

        private static string BuildPrompt(IReadOnlyList<NewsArticle> articles, MarketVerdictDto marketVerdict)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are helping a trading recommendation engine.");
            prompt.AppendLine("Focus only on news that can affect Bitcoin and the US Dollar over the short term.");
            prompt.AppendLine("Return strict JSON with properties sentiment, impactScore, recommendedAction, summary, and keyHeadlines.");
            prompt.AppendLine("sentiment must be one of: Neutral, Bullish, Bearish, Mixed.");
            prompt.AppendLine("impactScore must be a number from 0 to 100.");
            prompt.AppendLine("recommendedAction must be one of: Buy, Sell, Hold.");
            prompt.AppendLine("summary must be under 700 characters.");
            prompt.AppendLine("keyHeadlines must be an array of at most 5 strings.");
            prompt.AppendLine("Current technical context:");
            prompt.AppendLine($"Verdict: {marketVerdict?.Verdict}");
            prompt.AppendLine($"Confidence: {marketVerdict?.ConfidenceScore}");
            prompt.AppendLine($"TrendScore: {marketVerdict?.TrendScore}");
            prompt.AppendLine($"RSI: {marketVerdict?.Rsi}");
            prompt.AppendLine($"MACD Histogram: {marketVerdict?.MacdHistogram}");
            prompt.AppendLine($"ATR Percent: {marketVerdict?.AtrPercent}");
            prompt.AppendLine("Relevant news:");

            foreach (var article in articles.Take(10))
            {
                prompt.AppendLine(
                    $"- [{article.PublishedAt:O}] {article.Title} | Source={article.Category} | Summary={article.Summary}");
            }

            prompt.AppendLine("Do not invent facts beyond the provided items.");
            return prompt.ToString();
        }

        private static NewsRecommendationInsight BuildFallbackInsight(
            string focusKey,
            IReadOnlyList<NewsArticle> articles,
            MarketVerdictDto marketVerdict,
            string model,
            string rawPayload = null)
        {
            return new NewsRecommendationInsight
            {
                FocusKey = focusKey,
                Sentiment = NewsImpactSentiment.Neutral,
                ImpactScore = 35m,
                RecommendedAction = marketVerdict?.Verdict ?? MarketVerdict.Hold,
                Summary = BuildDefaultSummary(articles, marketVerdict),
                KeyHeadlines = articles?.Take(5).Select(x => x.Title).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                    ?? new List<string>(),
                Provider = "OpenAI",
                Model = model,
                GeneratedAt = DateTime.UtcNow,
                LatestArticlePublishedAt = articles != null && articles.Count > 0 ? articles.Max(x => x.PublishedAt) : (DateTime?)null,
                WasGenerated = false,
                RawPayloadJson = rawPayload
            };
        }

        private static string BuildDefaultSummary(IReadOnlyList<NewsArticle> articles, MarketVerdictDto marketVerdict)
        {
            if (articles == null || articles.Count == 0)
            {
                return "No recent cached Bitcoin or US Dollar headlines are available yet, so the recommendation remains technical-only for now.";
            }

            return $"Recent Bitcoin and US Dollar headlines are cached, but the news AI layer is using a fallback summary. " +
                $"Current technical bias remains {marketVerdict?.Verdict.ToString().ToLowerInvariant() ?? "hold"}.";
        }

        private static NewsImpactSentiment ParseSentiment(string sentiment)
        {
            if (Enum.TryParse(sentiment, true, out NewsImpactSentiment parsed))
            {
                return parsed;
            }

            return NewsImpactSentiment.Neutral;
        }

        private static MarketVerdict ParseAction(string action)
        {
            if (Enum.TryParse(action, true, out MarketVerdict parsed))
            {
                return parsed;
            }

            return MarketVerdict.Hold;
        }

        private static decimal Clamp(decimal value, decimal minimum, decimal maximum)
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }

        private static string ExtractResponseText(string responseContent)
        {
            using (var json = JsonDocument.Parse(responseContent))
            {
                if (json.RootElement.TryGetProperty("output_text", out var outputTextElement)
                    && outputTextElement.ValueKind == JsonValueKind.String)
                {
                    return outputTextElement.GetString();
                }

                if (json.RootElement.TryGetProperty("output", out var outputElement)
                    && outputElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in outputElement.EnumerateArray())
                    {
                        if (!item.TryGetProperty("content", out var contentElement) ||
                            contentElement.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var content in contentElement.EnumerateArray())
                        {
                            if (content.TryGetProperty("text", out var textElement) &&
                                textElement.ValueKind == JsonValueKind.String)
                            {
                                return textElement.GetString();
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
