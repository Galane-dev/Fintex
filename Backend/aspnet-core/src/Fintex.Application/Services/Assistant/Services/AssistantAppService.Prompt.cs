using Fintex.Investments.Assistant.Dto;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Investments.Assistant
{
    public partial class AssistantAppService
    {
        private async Task<AssistantPlan> BuildPlanAsync(AssistantChatInput input, AssistantContextSnapshot snapshot)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/responses";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            {
                return BuildFallbackPlan(input);
            }

            var payload = JsonSerializer.Serialize(new
            {
                model,
                input = BuildPrompt(input, snapshot)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseText = ExtractResponseText(await response.Content.ReadAsStringAsync());
            return string.IsNullOrWhiteSpace(responseText) ? BuildFallbackPlan(input) : ParsePlan(responseText, input);
        }

        private static string BuildPrompt(AssistantChatInput input, AssistantContextSnapshot snapshot)
        {
            var builder = new StringBuilder();
            builder.AppendLine("You are Fintex Copilot, a trading assistant inside the Fintex dashboard.");
            builder.AppendLine("Return strict JSON with properties: reply, voiceReply, suggestedPrompts, actions.");
            builder.AppendLine("Allowed action types: create_price_alert, get_recommendation, place_paper_trade, place_live_trade, refresh_behavior_analysis, sync_live_trades, create_goal_target, list_goal_targets, pause_goal_target, cancel_goal_target.");
            builder.AppendLine("Only create actions when the user explicitly asks you to do something.");
            builder.AppendLine("If trade quantity or destination is missing, ask a follow-up in reply and leave actions empty.");
            builder.AppendLine("Goal automation is best-effort only. Never promise guaranteed returns.");
            builder.AppendLine("For relative times like tomorrow or next week, convert them into a deadlineUtc ISO-8601 UTC timestamp using the client's timezone when available.");
            builder.AppendLine("For Binance market data and alerts, prefer BTCUSDT. For Alpaca live BTC trades, prefer BTCUSD.");
            builder.AppendLine($"Client timezone={input.ClientTimeZone ?? "unknown"}, clientNow={input.ClientNowIso ?? DateTime.UtcNow.ToString("O")}");
            builder.AppendLine($"Market verdict: {snapshot.Verdict?.Verdict}, confidence={snapshot.Verdict?.ConfidenceScore}, trend={snapshot.Verdict?.TrendScore}, price={snapshot.Verdict?.Price}");
            builder.AppendLine($"Paper account equity={snapshot.PaperSnapshot?.Account?.Equity}, openPositions={snapshot.PaperSnapshot?.Positions?.Count ?? 0}");
            builder.AppendLine($"Recommendation: {snapshot.Recommendation?.RecommendedAction}, risk={snapshot.Recommendation?.RiskScore}");
            builder.AppendLine($"Unread notifications={snapshot.Notifications?.UnreadCount}, activeAlerts={snapshot.Notifications?.AlertRules?.Items?.Count ?? 0}");
            builder.AppendLine($"Behavior score={snapshot.Profile?.BehavioralRiskScore}, summary={snapshot.Profile?.BehavioralSummary}");
            builder.AppendLine($"Tracked trades={snapshot.Trades.Count}");
            builder.AppendLine("Current goals:");
            foreach (var goal in snapshot.Goals.Take(4))
            {
                builder.AppendLine($"- id={goal.Id}, name={goal.Name}, status={goal.Status}, accountType={goal.AccountType}, progress={goal.ProgressPercent}, deadlineUtc={goal.DeadlineUtc}");
            }
            builder.AppendLine("Connected brokers:");

            foreach (var connection in snapshot.Connections.Where(x => x.IsActive))
            {
                builder.AppendLine($"- id={connection.Id}, name={connection.DisplayName}, provider={connection.Provider}, status={connection.Status}");
            }

            builder.AppendLine("Recent conversation:");
            foreach (var message in input.Conversation.TakeLast(10))
            {
                builder.AppendLine($"{message.Role}: {message.Content}");
            }

            builder.AppendLine($"user: {input.Message}");
            builder.AppendLine("Each action item may include: type, symbol, direction, quantity, targetPrice, stopLoss, takeProfit, connectionId, notifyEmail, notifyInApp, notes, goalId, goalName, accountType, targetType, targetPercent, targetAmount, deadlineUtc, maxAcceptableRisk, maxDrawdownPercent, maxPositionSizePercent, tradingSession, allowOvernightPositions.");
            builder.AppendLine("Keep reply concise and directly useful.");
            return builder.ToString();
        }

        private static AssistantPlan BuildFallbackPlan(AssistantChatInput input)
        {
            return new AssistantPlan
            {
                Reply = $"I can help with alerts, recommendations, trades, and explaining the dashboard. I understood: {input.Message}",
                VoiceReply = "I can help with alerts, recommendations, trades, and explaining the dashboard.",
                SuggestedPrompts =
                {
                    "Explain the current verdict.",
                    "Set a BTC alert at 70000.",
                    "Give me a recommendation right now.",
                    "Create a BTC growth goal for my paper account by tomorrow afternoon."
                }
            };
        }

        private static AssistantPlan ParsePlan(string responseText, AssistantChatInput input)
        {
            try
            {
                using var json = JsonDocument.Parse(responseText);
                var plan = new AssistantPlan
                {
                    Reply = ReadString(json.RootElement, "reply") ?? $"I understood: {input.Message}",
                    VoiceReply = ReadString(json.RootElement, "voiceReply")
                };

                if (json.RootElement.TryGetProperty("suggestedPrompts", out var prompts) && prompts.ValueKind == JsonValueKind.Array)
                {
                    plan.SuggestedPrompts.AddRange(prompts.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)).Take(4));
                }

                if (json.RootElement.TryGetProperty("actions", out var actions) && actions.ValueKind == JsonValueKind.Array)
                {
                    foreach (var action in actions.EnumerateArray().Take(3))
                    {
                        plan.Actions.Add(new AssistantPlannedAction
                        {
                            Type = ReadString(action, "type"),
                            Symbol = ReadString(action, "symbol"),
                            Direction = ReadString(action, "direction"),
                            Quantity = ReadDecimal(action, "quantity"),
                            TargetPrice = ReadDecimal(action, "targetPrice"),
                            StopLoss = ReadDecimal(action, "stopLoss"),
                            TakeProfit = ReadDecimal(action, "takeProfit"),
                            ConnectionId = ReadLong(action, "connectionId"),
                            GoalId = ReadLong(action, "goalId"),
                            GoalName = ReadString(action, "goalName"),
                            AccountType = ReadString(action, "accountType"),
                            TargetType = ReadString(action, "targetType"),
                            TargetPercent = ReadDecimal(action, "targetPercent"),
                            TargetAmount = ReadDecimal(action, "targetAmount"),
                            DeadlineUtc = ReadDateTime(action, "deadlineUtc"),
                            MaxAcceptableRisk = ReadDecimal(action, "maxAcceptableRisk"),
                            MaxDrawdownPercent = ReadDecimal(action, "maxDrawdownPercent"),
                            MaxPositionSizePercent = ReadDecimal(action, "maxPositionSizePercent"),
                            TradingSession = ReadString(action, "tradingSession"),
                            AllowOvernightPositions = ReadBool(action, "allowOvernightPositions"),
                            NotifyEmail = ReadBool(action, "notifyEmail"),
                            NotifyInApp = ReadBool(action, "notifyInApp"),
                            Notes = ReadString(action, "notes")
                        });
                    }
                }

                return plan;
            }
            catch (JsonException)
            {
                return BuildFallbackPlan(input);
            }
        }

        private static string ExtractResponseText(string responseContent)
        {
            using var json = JsonDocument.Parse(responseContent);
            if (json.RootElement.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
            {
                return outputText.GetString();
            }

            if (json.RootElement.TryGetProperty("output", out var outputArray) && outputArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in outputArray.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var contentArray) || contentArray.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var content in contentArray.EnumerateArray())
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

        private static string ReadString(JsonElement element, string name) =>
            element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

        private static decimal? ReadDecimal(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(property.GetString(), out var value) => value,
                _ => null
            };
        }

        private static long? ReadLong(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetInt64(),
                JsonValueKind.String when long.TryParse(property.GetString(), out var value) => value,
                _ => null
            };
        }

        private static bool? ReadBool(JsonElement element, string name) =>
            element.TryGetProperty(name, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False ? property.GetBoolean() : null;

        private static DateTime? ReadDateTime(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return DateTime.TryParse(
                property.GetString(),
                null,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                out var parsed)
                ? parsed
                : null;
        }
    }
}
