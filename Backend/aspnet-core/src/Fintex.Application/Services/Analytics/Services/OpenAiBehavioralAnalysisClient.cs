using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Analytics
{
    /// <summary>
    /// Uses the OpenAI Responses API to summarize behavioral trade patterns.
    /// </summary>
    public class OpenAiBehavioralAnalysisClient : IBehavioralAnalysisClient, ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAiBehavioralAnalysisClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<UserBehaviorInsight> AnalyzeAsync(UserProfile profile, IReadOnlyList<BehaviorTradeActivity> recentTrades, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/responses";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            {
                return new UserBehaviorInsight
                {
                    RiskScore = profile == null ? 0m : profile.BehavioralRiskScore,
                    Summary = "Behavioral AI analysis is not configured yet.",
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = false
                };
            }

            if (recentTrades == null || recentTrades.Count == 0)
            {
                return new UserBehaviorInsight
                {
                    RiskScore = profile == null ? 0m : profile.BehavioralRiskScore,
                    Summary = "There is not enough trade history yet to profile your behavior. Complete a few paper or live trades first.",
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = false
                };
            }

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Return strict JSON with properties riskScore and summary.");
            promptBuilder.AppendLine("riskScore must be a number between 0 and 100.");
            promptBuilder.AppendLine("summary must be under 500 characters and focus on trade discipline, drawdown behavior, and overtrading risk.");
            promptBuilder.AppendLine("User profile:");
            promptBuilder.AppendLine("PreferredBaseCurrency: " + (profile == null ? "USD" : profile.PreferredBaseCurrency));
            promptBuilder.AppendLine("RiskTolerance: " + (profile == null ? "50" : profile.RiskTolerance.ToString("0.####")));
            promptBuilder.AppendLine("StrategyNotes: " + (profile == null ? string.Empty : profile.StrategyNotes));
            promptBuilder.AppendLine("Recent trades:");

            foreach (var trade in recentTrades.Take(20))
            {
                promptBuilder.AppendLine(string.Format(
                    "{0} {1} {2} {3} qty={4} entry={5} exit={6} pnl={7} status={8}",
                    trade.Source,
                    trade.Symbol,
                    trade.AssetClass,
                    trade.Direction,
                    trade.Quantity,
                    trade.EntryPrice,
                    trade.ExitPrice,
                    trade.RealizedProfitLoss,
                    trade.Status));
            }

            var payload = JsonSerializer.Serialize(new
            {
                model = model,
                input = promptBuilder.ToString()
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
                return new UserBehaviorInsight
                {
                    RiskScore = profile == null ? 0m : profile.BehavioralRiskScore,
                    Summary = "OpenAI returned an empty behavioral analysis response.",
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = false
                };
            }

            return ParseInsight(responseText, profile, model);
        }

        private static UserBehaviorInsight ParseInsight(string responseText, UserProfile profile, string model)
        {
            var fallbackRiskScore = profile == null ? 0m : profile.BehavioralRiskScore;
            var normalizedResponse = NormalizeJsonText(responseText);

            if (TryParseInsightJson(normalizedResponse, out var riskScore, out var summary))
            {
                return new UserBehaviorInsight
                {
                    RiskScore = riskScore ?? fallbackRiskScore,
                    Summary = LimitSummary(summary ?? normalizedResponse),
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = true
                };
            }

            if (TryExtractInsightFromLooseText(normalizedResponse, out riskScore, out summary))
            {
                return new UserBehaviorInsight
                {
                    RiskScore = riskScore ?? fallbackRiskScore,
                    Summary = LimitSummary(summary ?? normalizedResponse),
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = true
                };
            }

            return new UserBehaviorInsight
            {
                RiskScore = fallbackRiskScore,
                Summary = LimitSummary(normalizedResponse),
                Provider = "OpenAI",
                Model = model,
                WasGenerated = true
            };
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
                        if (!item.TryGetProperty("content", out var contentElement)
                            || contentElement.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var content in contentElement.EnumerateArray())
                        {
                            if (content.TryGetProperty("text", out var textElement)
                                && textElement.ValueKind == JsonValueKind.String)
                            {
                                return textElement.GetString();
                            }
                        }
                    }
                }

                if (json.RootElement.TryGetProperty("choices", out var choicesElement)
                    && choicesElement.ValueKind == JsonValueKind.Array
                    && choicesElement.GetArrayLength() > 0)
                {
                    var firstChoice = choicesElement[0];
                    if (firstChoice.TryGetProperty("message", out var messageElement)
                        && messageElement.TryGetProperty("content", out var contentElement)
                        && contentElement.ValueKind == JsonValueKind.String)
                    {
                        return contentElement.GetString();
                    }
                }
            }

            return null;
        }

        private static string NormalizeJsonText(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return responseText;
            }

            var trimmed = responseText.Trim();
            if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                return trimmed;
            }

            var lines = trimmed.Split('\n');
            if (lines.Length <= 2)
            {
                return trimmed;
            }

            return string.Join("\n", lines[1..^1]).Trim();
        }

        private static bool TryParseInsightJson(string responseText, out decimal? riskScore, out string summary)
        {
            riskScore = null;
            summary = null;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return false;
            }

            if (TryParseInsightJsonCandidate(responseText, out riskScore, out summary))
            {
                return true;
            }

            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return false;
            }

            var jsonCandidate = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            return TryParseInsightJsonCandidate(jsonCandidate, out riskScore, out summary);
        }

        private static bool TryParseInsightJsonCandidate(string jsonCandidate, out decimal? riskScore, out string summary)
        {
            riskScore = null;
            summary = null;

            try
            {
                using var json = JsonDocument.Parse(jsonCandidate);
                riskScore = ReadDecimal(json.RootElement, "riskScore");
                summary = ReadString(json.RootElement, "summary");
                return riskScore.HasValue || !string.IsNullOrWhiteSpace(summary);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static bool TryExtractInsightFromLooseText(string responseText, out decimal? riskScore, out string summary)
        {
            riskScore = null;
            summary = null;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return false;
            }

            var riskMatch = Regex.Match(
                responseText,
                "\"riskScore\"\\s*:\\s*(?<value>-?\\d+(?:\\.\\d+)?)",
                RegexOptions.IgnoreCase);
            if (riskMatch.Success &&
                decimal.TryParse(riskMatch.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedRiskScore))
            {
                riskScore = parsedRiskScore;
            }

            var summaryMatch = Regex.Match(
                responseText,
                "\"summary\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"\\\\])*)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (summaryMatch.Success)
            {
                summary = Regex.Unescape(summaryMatch.Groups["value"].Value).Trim();
            }

            return riskScore.HasValue || !string.IsNullOrWhiteSpace(summary);
        }

        private static decimal? ReadDecimal(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }

            if (value.ValueKind == JsonValueKind.String &&
                decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimalValue))
            {
                return decimalValue;
            }

            return null;
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return value.GetString();
        }

        private static string LimitSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return summary;
            }

            var trimmed = summary.Trim();
            return trimmed.Length <= 500 ? trimmed : trimmed.Substring(0, 500);
        }
    }
}
