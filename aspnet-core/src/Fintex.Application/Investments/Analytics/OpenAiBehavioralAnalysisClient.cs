using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

        public async Task<UserBehaviorInsight> AnalyzeAsync(UserProfile profile, IReadOnlyList<Trade> recentTrades, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/responses";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model) || recentTrades == null || recentTrades.Count == 0)
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
                    "{0} {1} {2} qty={3} entry={4} exit={5} pnl={6} status={7}",
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

            try
            {
                using (var json = JsonDocument.Parse(responseText))
                {
                    var riskScore = json.RootElement.TryGetProperty("riskScore", out var riskElement)
                        ? riskElement.GetDecimal()
                        : 0m;
                    var summary = json.RootElement.TryGetProperty("summary", out var summaryElement)
                        ? summaryElement.GetString()
                        : responseText;

                    return new UserBehaviorInsight
                    {
                        RiskScore = riskScore,
                        Summary = summary,
                        Provider = "OpenAI",
                        Model = model,
                        WasGenerated = true
                    };
                }
            }
            catch (JsonException)
            {
                return new UserBehaviorInsight
                {
                    RiskScore = profile == null ? 0m : profile.BehavioralRiskScore,
                    Summary = responseText.Length > 500 ? responseText.Substring(0, 500) : responseText,
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = true
                };
            }
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
    }
}
