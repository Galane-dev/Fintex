using Abp.UI;
using Fintex.Investments.Assistant.Dto;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Investments.Assistant
{
    public partial class AssistantAppService
    {
        public async Task<AssistantRealtimeSessionDto> CreateRealtimeVoiceSessionAsync(AssistantRealtimeSessionInput input)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new UserFriendlyException("OpenAI is not configured for Realtime voice sessions.");
            }

            var model = _configuration["OpenAI:RealtimeModel"];
            if (string.IsNullOrWhiteSpace(model))
            {
                model = "gpt-realtime";
            }

            var voice = _configuration["OpenAI:RealtimeVoice"];
            if (string.IsNullOrWhiteSpace(voice))
            {
                voice = "marin";
            }

            var snapshot = await LoadContextSnapshotAsync();
            var instructions = BuildRealtimeInstructions(snapshot, input);
            var endpoint = _configuration["OpenAI:RealtimeClientSecretsEndpoint"] ?? "https://api.openai.com/v1/realtime/client_secrets";

            var payload = JsonSerializer.Serialize(new
            {
                session = new
                {
                    type = "realtime",
                    model,
                    instructions,
                    // The client_secrets endpoint does not allow both text and audio output at once.
                    // Audio sessions still emit transcripts over the Realtime event stream.
                    output_modalities = new[] { "audio" },
                    audio = new
                    {
                        output = new
                        {
                            voice
                        }
                    }
                }
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new UserFriendlyException(BuildRealtimeSessionErrorMessage(errorContent));
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;
            var clientSecret = ReadClientSecret(root);

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new UserFriendlyException("OpenAI did not return a browser-safe Realtime client secret.");
            }

            return new AssistantRealtimeSessionDto
            {
                ClientSecret = clientSecret,
                ExpiresAtUtc = ReadExpiresAt(root),
                Model = model,
                Voice = voice,
                Instructions = instructions
            };
        }

        private static string BuildRealtimeInstructions(AssistantContextSnapshot snapshot, AssistantRealtimeSessionInput input)
        {
            var builder = new StringBuilder();
            builder.AppendLine("You are Fintex Copilot inside a live voice chat.");
            builder.AppendLine("Speak naturally, warmly, and briefly. Never read JSON, markdown, or tool payloads aloud.");
            builder.AppendLine("Use the available tools to inspect current dashboard state before answering time-sensitive questions or taking actions.");
            builder.AppendLine("Use the available tools before placing trades, creating alerts, changing goals, or changing automation rules.");
            builder.AppendLine("If a required ID, quantity, or destination is missing, ask one short follow-up question.");
            builder.AppendLine("Do not invent broker connection ids, goal ids, notification ids, or position ids.");
            builder.AppendLine("When an action succeeds, confirm it in one spoken sentence and stop. Keep voice replies concise.");
            builder.AppendLine("Only discuss BTC market data unless the user explicitly asks for another instrument supported by the app.");
            builder.AppendLine("This voice session is direct browser-to-OpenAI audio. Keep latency low by avoiding long monologues.");
            builder.AppendLine($"Client timezone: {input?.ClientTimeZone ?? "unknown"}.");
            builder.AppendLine($"Client current time: {input?.ClientNowIso ?? DateTime.UtcNow.ToString("O")}.");
            builder.AppendLine("Current Fintex context snapshot:");
            builder.AppendLine($"- Verdict: {snapshot.Verdict?.Verdict}, confidence {snapshot.Verdict?.ConfidenceScore:0.##}, trend {snapshot.Verdict?.TrendScore:0.##}, price {snapshot.Verdict?.Price:0.##}.");
            builder.AppendLine($"- Paper equity: {snapshot.PaperSnapshot?.Account?.Equity:0.##}, open paper positions: {snapshot.PaperSnapshot?.Positions?.Count ?? 0}.");
            builder.AppendLine($"- Recommendation: {snapshot.Recommendation?.RecommendedAction}, suggested trade action {snapshot.Recommendation?.SuggestedTradeAction}, risk {snapshot.Recommendation?.RiskScore:0.##}.");
            builder.AppendLine($"- Unread notifications: {snapshot.Notifications?.UnreadCount ?? 0}, alert rules: {snapshot.Notifications?.AlertRules?.Items?.Count ?? 0}.");
            builder.AppendLine($"- Goal targets: {snapshot.Goals.Count}, automation rules: {snapshot.AutomationRules.Count}, live trades: {snapshot.Trades.Count}.");
            builder.AppendLine($"- Connected brokers: {snapshot.Connections.FindAll(x => x.IsActive).Count}.");
            builder.AppendLine($"- Behavior score: {snapshot.Profile?.BehavioralRiskScore:0.##}. Summary: {TrimForVoice(snapshot.Profile?.BehavioralSummary, 220)}");
            builder.AppendLine($"- Macro risk: {snapshot.MacroInsight?.RiskScore:0.##}. Next event: {snapshot.MacroInsight?.NextEventAtUtc ?? "none"}.");

            if (snapshot.StrategyValidations.Count > 0)
            {
                var latestValidation = snapshot.StrategyValidations[0];
                builder.AppendLine($"- Latest strategy validation: {latestValidation.Outcome} with score {latestValidation.ValidationScore:0.##}. {TrimForVoice(latestValidation.Summary, 180)}");
            }

            builder.AppendLine("Prefer the higher-quality voice persona and keep the conversation sounding human, not robotic.");
            return builder.ToString();
        }

        private static string TrimForVoice(string value, int limit)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "No summary available.";
            }

            return value.Length <= limit ? value : $"{value.Substring(0, limit).TrimEnd()}...";
        }

        private static string ReadNestedString(JsonElement root, string parentName, string propertyName)
        {
            if (!root.TryGetProperty(parentName, out var parent))
            {
                return null;
            }

            return parent.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }

        private static string ReadClientSecret(JsonElement root)
        {
            var nestedValue = ReadNestedString(root, "client_secret", "value");
            if (!string.IsNullOrWhiteSpace(nestedValue))
            {
                return nestedValue;
            }

            return root.TryGetProperty("value", out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }

        private static string ReadExpiresAt(JsonElement root)
        {
            if (root.TryGetProperty("expires_at", out var topLevelExpiresAt))
            {
                return FormatExpiresAt(topLevelExpiresAt);
            }

            if (!root.TryGetProperty("client_secret", out var clientSecret) || !clientSecret.TryGetProperty("expires_at", out var expiresAt))
            {
                return null;
            }

            return FormatExpiresAt(expiresAt);
        }

        private static string FormatExpiresAt(JsonElement expiresAt)
        {
            if (expiresAt.ValueKind == JsonValueKind.String)
            {
                return expiresAt.GetString();
            }

            if (expiresAt.ValueKind == JsonValueKind.Number && expiresAt.TryGetInt64(out var epochSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(epochSeconds).UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
            }

            return null;
        }

        private static string BuildRealtimeSessionErrorMessage(string errorContent)
        {
            const string fallback = "OpenAI did not accept the Realtime session request.";

            if (string.IsNullOrWhiteSpace(errorContent))
            {
                return fallback;
            }

            try
            {
                using var json = JsonDocument.Parse(errorContent);
                if (json.RootElement.TryGetProperty("error", out var error))
                {
                    if (error.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                    {
                        return $"OpenAI did not accept the Realtime session request: {message.GetString()}";
                    }

                    if (error.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String)
                    {
                        return $"OpenAI did not accept the Realtime session request: {type.GetString()}";
                    }
                }
            }
            catch (JsonException)
            {
                // Fall back to a generic message when the upstream error body is not JSON.
            }

            return fallback;
        }
    }
}
