using Abp.Dependency;
using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.News;
using Fintex.Investments.Profiles.Dto;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.Strategies
{
    /// <summary>
    /// Uses OpenAI to evaluate whether a user strategy is coherent with the live market context.
    /// </summary>
    public class OpenAiStrategyValidationClient : IStrategyValidationClient, ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAiStrategyValidationClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<StrategyValidationInsight> ValidateAsync(
            ValidateStrategyRequest request,
            MarketVerdictDto marketVerdict,
            NewsRecommendationInsight newsInsight,
            UserProfileDto profile,
            CancellationToken cancellationToken)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/responses";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            {
                return BuildFallbackInsight(
                    request,
                    marketVerdict,
                    newsInsight,
                    profile,
                    model,
                    "Fallback validation was used because the AI validator is not configured.");
            }

            var payload = JsonSerializer.Serialize(new
            {
                model,
                input = BuildPrompt(request, marketVerdict, newsInsight, profile)
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseText = ExtractResponseText(await response.Content.ReadAsStringAsync(cancellationToken));
            return string.IsNullOrWhiteSpace(responseText)
                ? BuildFallbackInsight(
                    request,
                    marketVerdict,
                    newsInsight,
                    profile,
                    model,
                    "Fallback validation was used because the AI validator returned an empty response.")
                : ParseInsight(responseText, model);
        }

        private static string BuildPrompt(
            ValidateStrategyRequest request,
            MarketVerdictDto marketVerdict,
            NewsRecommendationInsight newsInsight,
            UserProfileDto profile)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Return strict JSON with properties score, outcome, summary, strengths, risks, improvements, suggestedAction, suggestedEntryPrice, suggestedStopLoss, suggestedTakeProfit.");
            prompt.AppendLine("score must be between 0 and 100.");
            prompt.AppendLine("outcome must be one of: Validated, Caution, Fail.");
            prompt.AppendLine("strengths, risks, improvements must each be short arrays of strings.");
            prompt.AppendLine("Keep summary under 500 characters.");
            prompt.AppendLine($"Strategy name: {request.StrategyName}");
            prompt.AppendLine($"Symbol: {request.Symbol}");
            prompt.AppendLine($"Timeframe: {request.Timeframe}");
            prompt.AppendLine($"Direction preference: {request.DirectionPreference}");
            prompt.AppendLine("Strategy text:");
            prompt.AppendLine(request.StrategyText);
            prompt.AppendLine($"Market verdict: {marketVerdict?.Verdict}, confidence={marketVerdict?.ConfidenceScore}, trend={marketVerdict?.TrendScore}, price={marketVerdict?.Price}");
            prompt.AppendLine($"Indicators: sma={marketVerdict?.Sma}, ema={marketVerdict?.Ema}, rsi={marketVerdict?.Rsi}, macd={marketVerdict?.Macd}, adx={marketVerdict?.Adx}, atrPercent={marketVerdict?.AtrPercent}");
            prompt.AppendLine($"News: sentiment={newsInsight?.Sentiment}, impact={newsInsight?.ImpactScore}, action={newsInsight?.RecommendedAction}, summary={newsInsight?.Summary}");
            prompt.AppendLine($"User risk tolerance={profile?.RiskTolerance}, behavioral score={profile?.BehavioralRiskScore}, strategy notes={profile?.StrategyNotes}");
            prompt.AppendLine("Score the strategy based on clarity, risk definition, alignment with market conditions, and whether it should be traded right now.");
            return prompt.ToString();
        }

        private static StrategyValidationInsight ParseInsight(string responseText, string model)
        {
            var normalizedResponse = NormalizeJsonText(responseText);

            try
            {
                using var json = JsonDocument.Parse(normalizedResponse);
                return new StrategyValidationInsight
                {
                    ValidationScore = json.RootElement.GetProperty("score").GetDecimal(),
                    Outcome = ParseOutcome(json.RootElement.GetProperty("outcome").GetString()),
                    Summary = ReadString(json.RootElement, "summary"),
                    Strengths = ReadList(json.RootElement, "strengths"),
                    Risks = ReadList(json.RootElement, "risks"),
                    Improvements = ReadList(json.RootElement, "improvements"),
                    SuggestedAction = ReadString(json.RootElement, "suggestedAction"),
                    SuggestedEntryPrice = ReadDecimal(json.RootElement, "suggestedEntryPrice"),
                    SuggestedStopLoss = ReadDecimal(json.RootElement, "suggestedStopLoss"),
                    SuggestedTakeProfit = ReadDecimal(json.RootElement, "suggestedTakeProfit"),
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = true
                };
            }
            catch (JsonException)
            {
                return new StrategyValidationInsight
                {
                    ValidationScore = 50m,
                    Outcome = StrategyValidationOutcome.Caution,
                    Summary = normalizedResponse,
                    Provider = "OpenAI",
                    Model = model,
                    WasGenerated = true
                };
            }
        }

        private static StrategyValidationInsight BuildFallbackInsight(
            ValidateStrategyRequest request,
            MarketVerdictDto marketVerdict,
            NewsRecommendationInsight newsInsight,
            UserProfileDto profile,
            string model,
            string summary)
        {
            var hasStops = request.StrategyText?.ToLowerInvariant().Contains("stop") == true;
            var hasTarget = request.StrategyText?.ToLowerInvariant().Contains("target") == true ||
                request.StrategyText?.ToLowerInvariant().Contains("take profit") == true;
            var score = 45m;
            score += hasStops ? 20m : -10m;
            score += hasTarget ? 15m : -5m;
            score += marketVerdict?.ConfidenceScore >= 60m ? 10m : 0m;
            score += profile?.BehavioralRiskScore <= 40m ? 5m : 0m;
            score -= newsInsight?.ImpactScore >= 70m ? 5m : 0m;

            return new StrategyValidationInsight
            {
                ValidationScore = score < 0m ? 0m : score > 100m ? 100m : score,
                Outcome = score >= 75m ? StrategyValidationOutcome.Validated : score >= 50m ? StrategyValidationOutcome.Caution : StrategyValidationOutcome.Fail,
                Summary = $"{summary} Add clear entry, stop loss, and take profit rules for stronger validation.",
                Strengths = hasStops || hasTarget ? new List<string> { "Your strategy already includes some risk structure." } : new List<string>(),
                Risks = new List<string> { "Fallback mode is less nuanced than the AI validator." },
                Improvements = new List<string>
                {
                    "Specify the entry trigger more clearly.",
                    "Define both stop loss and take profit.",
                    "State which market condition invalidates the setup."
                },
                SuggestedAction = marketVerdict?.Verdict.ToString() ?? "Hold",
                SuggestedEntryPrice = marketVerdict?.Price,
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

            if (json.RootElement.TryGetProperty("output", out var outputElement) &&
                outputElement.ValueKind == JsonValueKind.Array)
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

            if (json.RootElement.TryGetProperty("choices", out var choicesElement) &&
                choicesElement.ValueKind == JsonValueKind.Array &&
                choicesElement.GetArrayLength() > 0)
            {
                var firstChoice = choicesElement[0];
                if (firstChoice.TryGetProperty("message", out var messageElement) &&
                    messageElement.TryGetProperty("content", out var contentElement) &&
                    contentElement.ValueKind == JsonValueKind.String)
                {
                    return contentElement.GetString();
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
            if (!trimmed.StartsWith("```"))
            {
                return trimmed;
            }

            var lines = trimmed.Split('\n');
            if (lines.Length <= 2)
            {
                return trimmed;
            }

            var startIndex = 1;
            var endIndex = lines.Length - 1;
            return string.Join("\n", lines[startIndex..endIndex]).Trim();
        }

        private static StrategyValidationOutcome ParseOutcome(string value) =>
            value switch
            {
                "Validated" => StrategyValidationOutcome.Validated,
                "Fail" => StrategyValidationOutcome.Fail,
                _ => StrategyValidationOutcome.Caution
            };

        private static string ReadString(JsonElement element, string name) =>
            element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

        private static decimal? ReadDecimal(JsonElement element, string name) =>
            element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDecimal() : null;

        private static List<string> ReadList(JsonElement element, string name)
        {
            var items = new List<string>();
            if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                return items;
            }

            foreach (var item in property.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    items.Add(item.GetString());
                }
            }

            return items;
        }
    }
}
