using Abp.Dependency;
using Fintex.Investments.Trading.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Trading
{
    /// <summary>
    /// Generates coaching notes for closed trades from execution-time context and recent trade behavior.
    /// </summary>
    public class TradeReviewService : ITradeReviewService, ITransientDependency
    {
        private const int MaxRecentTrades = 8;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TradeReviewService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<IReadOnlyDictionary<long, ClosedTradeReviewDto>> BuildClosedTradeReviewsAsync(
            IReadOnlyList<Trade> closedTrades,
            IReadOnlyList<Trade> recentTrades,
            IReadOnlyDictionary<long, TradeExecutionContext> executionContexts,
            UserProfile profile,
            CancellationToken cancellationToken)
        {
            if (closedTrades == null || closedTrades.Count == 0)
            {
                return new Dictionary<long, ClosedTradeReviewDto>();
            }

            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            {
                return closedTrades.ToDictionary(trade => trade.Id, trade => BuildFallbackReview(trade, recentTrades, executionContexts, model));
            }

            var prompt = BuildPrompt(closedTrades, recentTrades, executionContexts, profile);
            var reviews = await RequestReviewsAsync(apiKey, model, prompt, cancellationToken);
            if (reviews.Count == 0)
            {
                return closedTrades.ToDictionary(trade => trade.Id, trade => BuildFallbackReview(trade, recentTrades, executionContexts, model));
            }

            return closedTrades.ToDictionary(
                trade => trade.Id,
                trade => reviews.TryGetValue(trade.Id, out var review)
                    ? review
                    : BuildFallbackReview(trade, recentTrades, executionContexts, model));
        }

        private async Task<Dictionary<long, ClosedTradeReviewDto>> RequestReviewsAsync(string apiKey, string model, string prompt, CancellationToken cancellationToken)
        {
            var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/responses";
            var payload = JsonSerializer.Serialize(new { model, input = prompt });
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseText = ExtractResponseText(await response.Content.ReadAsStringAsync(cancellationToken));
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new Dictionary<long, ClosedTradeReviewDto>();
            }

            try
            {
                var reviews = new Dictionary<long, ClosedTradeReviewDto>();
                using var json = JsonDocument.Parse(NormalizeJsonText(responseText));
                foreach (var item in json.RootElement.GetProperty("reviews").EnumerateArray())
                {
                    var tradeId = item.GetProperty("tradeId").GetInt64();
                    reviews[tradeId] = new ClosedTradeReviewDto
                    {
                        Good = ReadString(item, "good"),
                        Bad = ReadString(item, "bad"),
                        RepeatedPattern = ReadString(item, "repeatedPattern"),
                        Provider = "OpenAI",
                        Model = model,
                        WasGenerated = true
                    };
                }

                return reviews;
            }
            catch (JsonException)
            {
                return new Dictionary<long, ClosedTradeReviewDto>();
            }
        }

        private static string BuildPrompt(
            IReadOnlyList<Trade> closedTrades,
            IReadOnlyList<Trade> recentTrades,
            IReadOnlyDictionary<long, TradeExecutionContext> executionContexts,
            UserProfile profile)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Return strict JSON with a single property named reviews.");
            builder.AppendLine("reviews must be an array of objects with tradeId, good, bad, repeatedPattern.");
            builder.AppendLine("Keep each field under 260 characters and practical.");
            builder.AppendLine($"User behavioral score={profile?.BehavioralRiskScore}, summary={profile?.BehavioralSummary}, strategy notes={profile?.StrategyNotes}.");
            builder.AppendLine("Recent trade pattern sample:");

            foreach (var trade in recentTrades.OrderByDescending(item => item.ClosedAt ?? item.ExecutedAt).Take(MaxRecentTrades))
            {
                builder.AppendLine($"Recent trade id={trade.Id}, direction={trade.Direction}, pnl={trade.RealizedProfitLoss}, status={trade.Status}, stop={trade.StopLoss}, target={trade.TakeProfit}.");
            }

            builder.AppendLine("Closed trades to review:");
            foreach (var trade in closedTrades)
            {
                executionContexts.TryGetValue(trade.Id, out var context);
                builder.AppendLine($"Trade {trade.Id}: symbol={trade.Symbol}, direction={trade.Direction}, entry={trade.EntryPrice}, exit={trade.ExitPrice}, pnl={trade.RealizedProfitLoss}, riskScore={trade.CurrentRiskScore}, recommendation={trade.CurrentRecommendation}, summary={trade.CurrentAnalysisSummary}.");
                if (context == null)
                {
                    builder.AppendLine("Execution context unavailable.");
                    continue;
                }

                builder.AppendLine($"Execution context: verdict={context.MarketVerdict}, confidence={context.ConfidenceScore}, trend={context.TrendScore}, rsi={context.Rsi}, macdHistogram={context.MacdHistogram}, atrPercent={context.AtrPercent}, adx={context.Adx}, spreadPercent={context.SpreadPercent}, stop={context.StopLoss}, target={context.TakeProfit}, behaviorSummary={context.UserBehavioralSummary}, decisionSummary={context.DecisionSummary}.");
            }

            builder.AppendLine("For each trade, explain one good thing, one weak point, and one repeating behavioral pattern using the stored market context when available.");
            return builder.ToString();
        }

        private static ClosedTradeReviewDto BuildFallbackReview(
            Trade trade,
            IReadOnlyList<Trade> recentTrades,
            IReadOnlyDictionary<long, TradeExecutionContext> executionContexts,
            string model)
        {
            executionContexts.TryGetValue(trade.Id, out var context);
            var sameDirection = recentTrades.Count(item => item.Direction == trade.Direction && item.Status == TradeStatus.Closed);
            var repeatedLosses = recentTrades.Count(item => item.Status == TradeStatus.Closed && item.RealizedProfitLoss < 0m);
            var hasStructuredPlan = (trade.StopLoss ?? context?.StopLoss).HasValue && (trade.TakeProfit ?? context?.TakeProfit).HasValue;
            var wasAligned = string.Equals(context?.MarketVerdict, trade.Direction.ToString(), StringComparison.OrdinalIgnoreCase);

            return new ClosedTradeReviewDto
            {
                Good = trade.RealizedProfitLoss >= 0m
                    ? "This trade still ended positive, so some part of the read or management worked."
                    : hasStructuredPlan
                        ? "You traded with a defined structure, which is better than taking an unmanaged position."
                        : "You now have another finished trade to review instead of guessing from memory.",
                Bad = trade.RealizedProfitLoss < 0m
                    ? "This trade closed red, so the entry or the management did not hold up in live conditions."
                    : !hasStructuredPlan
                        ? "The setup did not carry a full stop-loss and take-profit plan, which weakens consistency."
                        : wasAligned
                            ? "The market context was supportive, but the trade still left room for better execution."
                            : "The trade did not fully align with the stored market context at execution time.",
                RepeatedPattern = repeatedLosses >= 3
                    ? "Recent losses are clustering. The pattern to fix is forcing setups before the edge is clear enough."
                    : sameDirection >= 3
                        ? $"You repeatedly lean toward {trade.Direction.ToString().ToLowerInvariant()} trades. Make sure that is evidence-driven, not a standing bias."
                        : "Your pattern is still forming. Keep repeating only setups with clear structure and defined risk.",
                Provider = "OpenAI",
                Model = model,
                WasGenerated = false
            };
        }

        private static string ExtractResponseText(string responseContent)
        {
            using var json = JsonDocument.Parse(responseContent);
            if (json.RootElement.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
            {
                return outputText.GetString();
            }

            if (json.RootElement.TryGetProperty("output", out var outputElement) && outputElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in outputElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var content in contentElement.EnumerateArray())
                    {
                        if (content.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                        {
                            return textElement.GetString();
                        }
                    }
                }
            }

            return null;
        }

        private static string NormalizeJsonText(string responseText)
        {
            var trimmed = responseText?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || !trimmed.StartsWith("```"))
            {
                return trimmed;
            }

            var lines = trimmed.Split('\n');
            return lines.Length <= 2 ? trimmed : string.Join("\n", lines[1..^1]).Trim();
        }

        private static string ReadString(JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
    }
}
