using Abp.Application.Services.Dto;
using Fintex.Investments.Assistant.Dto;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Goals;
using Fintex.Investments.Goals.Dto;
using Fintex.Investments.Notifications.Dto;
using Fintex.Investments.PaperTrading.Dto;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Fintex.Investments.Assistant
{
    public partial class AssistantAppService
    {
        private async Task<List<AssistantActionResultDto>> ExecuteActionsAsync(IReadOnlyList<AssistantPlannedAction> actions)
        {
            var results = new List<AssistantActionResultDto>();
            foreach (var action in actions.Where(x => !string.IsNullOrWhiteSpace(x.Type)))
            {
                results.Add(await ExecuteActionAsync(action));
            }

            return results;
        }

        private async Task<AssistantActionResultDto> ExecuteActionAsync(AssistantPlannedAction action)
        {
            try
            {
                return action.Type switch
                {
                    "create_price_alert" => await CreatePriceAlertAsync(action),
                    "get_recommendation" => await GetRecommendationAsync(action),
                    "place_paper_trade" => await PlacePaperTradeAsync(action),
                    "place_live_trade" => await PlaceLiveTradeAsync(action),
                    "refresh_behavior_analysis" => await RefreshBehaviorAsync(),
                    "sync_live_trades" => await SyncLiveTradesAsync(),
                    "create_goal_target" => await CreateGoalTargetAsync(action),
                    "list_goal_targets" => await ListGoalTargetsAsync(),
                    "pause_goal_target" => await PauseGoalTargetAsync(action),
                    "cancel_goal_target" => await CancelGoalTargetAsync(action),
                    _ => BuildActionResult(action.Type, "ignored", "Action skipped", "I left that action untouched because it is not supported yet.")
                };
            }
            catch (System.Exception exception)
            {
                return BuildActionResult(action.Type, "failed", "Action failed", exception.Message);
            }
        }

        private async Task<AssistantActionResultDto> CreatePriceAlertAsync(AssistantPlannedAction action)
        {
            if (!action.TargetPrice.HasValue)
            {
                return BuildActionResult(action.Type, "needs_input", "Alert needs a price", "Tell me the exact target price you want for the BTC alert.");
            }

            var rule = await _notificationAppService.CreatePriceAlertAsync(new CreatePriceAlertInput
            {
                Name = string.IsNullOrWhiteSpace(action.Notes) ? "Assistant BTC alert" : action.Notes,
                Symbol = NormalizeAlertSymbol(action.Symbol),
                Provider = MarketDataProvider.Binance,
                TargetPrice = action.TargetPrice.Value,
                NotifyEmail = action.NotifyEmail ?? true,
                NotifyInApp = action.NotifyInApp ?? true,
                Notes = action.Notes
            });

            return BuildActionResult(action.Type, "completed", "Alert created", $"I created a {rule.Symbol} alert at {rule.TargetPrice:0.00}.");
        }

        private async Task<AssistantActionResultDto> GetRecommendationAsync(AssistantPlannedAction action)
        {
            var recommendation = await _paperTradingAppService.GetRecommendationAsync(new GetPaperTradeRecommendationInput
            {
                Symbol = NormalizeAlertSymbol(action.Symbol),
                Provider = MarketDataProvider.Binance,
                AssetClass = AssetClass.Crypto,
                Quantity = action.Quantity,
                StopLoss = action.StopLoss,
                TakeProfit = action.TakeProfit
            });

            return BuildActionResult(action.Type, "completed", "Recommendation ready", $"{recommendation.Headline} Risk {recommendation.RiskScore:0.0}. {recommendation.Summary}");
        }

        private async Task<AssistantActionResultDto> PlacePaperTradeAsync(AssistantPlannedAction action)
        {
            if (!action.Quantity.HasValue || action.Quantity.Value <= 0m || string.IsNullOrWhiteSpace(action.Direction))
            {
                return BuildActionResult(action.Type, "needs_input", "Paper trade needs details", "Tell me both the side and quantity for the paper trade.");
            }

            var result = await _paperTradingAppService.PlaceMarketOrderAsync(new PlacePaperOrderInput
            {
                Symbol = NormalizeAlertSymbol(action.Symbol),
                Provider = MarketDataProvider.Binance,
                AssetClass = AssetClass.Crypto,
                Direction = ParseDirection(action.Direction),
                Quantity = action.Quantity.Value,
                StopLoss = action.StopLoss,
                TakeProfit = action.TakeProfit,
                Notes = action.Notes
            });

            return result.WasExecuted
                ? BuildActionResult(action.Type, "completed", "Paper trade placed", $"Paper {result.Order.Direction} {result.Order.Quantity:0.########} {result.Order.Symbol} at {result.Order.ExecutedPrice:0.00}.")
                : BuildActionResult(action.Type, "blocked", "Paper trade blocked", result.Assessment?.Summary ?? "The simulator blocked that trade.");
        }

        private async Task<AssistantActionResultDto> PlaceLiveTradeAsync(AssistantPlannedAction action)
        {
            if (!action.Quantity.HasValue || action.Quantity.Value <= 0m || string.IsNullOrWhiteSpace(action.Direction))
            {
                return BuildActionResult(action.Type, "needs_input", "Live trade needs details", "Tell me the side, quantity, and broker account for the live trade.");
            }

            var connectionId = action.ConnectionId ?? await ResolveDefaultConnectionIdAsync();
            if (!connectionId.HasValue)
            {
                return BuildActionResult(action.Type, "needs_input", "Broker account needed", "Connect Alpaca first, or tell me which connected broker account to use.");
            }

            var execution = await _externalBrokerTradingAppService.PlaceMarketOrderAsync(new PlaceExternalBrokerMarketOrderInput
            {
                ConnectionId = connectionId.Value,
                Symbol = NormalizeLiveSymbol(action.Symbol),
                Provider = MarketDataProvider.Binance,
                AssetClass = AssetClass.Crypto,
                Direction = ParseDirection(action.Direction),
                Quantity = action.Quantity.Value,
                StopLoss = action.StopLoss,
                TakeProfit = action.TakeProfit,
                Notes = action.Notes
            });

            return BuildActionResult(action.Type, "completed", "Live trade sent", $"{execution.BrokerName} accepted a {execution.Trade.Direction} order for {execution.Trade.Quantity:0.########} {execution.BrokerSymbol}.");
        }

        private async Task<AssistantActionResultDto> RefreshBehaviorAsync()
        {
            var profile = await _userProfileAppService.GetMyProfileAsync();
            return BuildActionResult("refresh_behavior_analysis", "completed", "Behavior profile loaded", profile?.BehavioralSummary ?? "Behavior profile is available.");
        }

        private async Task<AssistantActionResultDto> SyncLiveTradesAsync()
        {
            var result = await _externalBrokerTradingAppService.SyncMyConnectionsAsync();
            return BuildActionResult("sync_live_trades", "completed", "Broker sync finished", $"Imported {result.ImportedTrades} trades and updated {result.UpdatedTrades}.");
        }

        private async Task<AssistantActionResultDto> CreateGoalTargetAsync(AssistantPlannedAction action)
        {
            if (!action.DeadlineUtc.HasValue || string.IsNullOrWhiteSpace(action.AccountType) || string.IsNullOrWhiteSpace(action.TargetType))
            {
                return BuildActionResult(action.Type, "needs_input", "Goal needs details", "Tell me the account type, target type, and exact deadline for the BTC goal.");
            }

            var input = new CreateGoalTargetInput
            {
                Name = string.IsNullOrWhiteSpace(action.GoalName) ? action.Notes : action.GoalName,
                AccountType = ParseGoalAccountType(action.AccountType),
                ExternalConnectionId = action.ConnectionId,
                TargetType = ParseGoalTargetType(action.TargetType),
                TargetPercent = action.TargetPercent,
                TargetAmount = action.TargetAmount,
                DeadlineUtc = action.DeadlineUtc.Value,
                MaxAcceptableRisk = action.MaxAcceptableRisk ?? 45m,
                MaxDrawdownPercent = action.MaxDrawdownPercent ?? 2.5m,
                MaxPositionSizePercent = action.MaxPositionSizePercent ?? 20m,
                TradingSession = ParseGoalTradingSession(action.TradingSession),
                AllowOvernightPositions = action.AllowOvernightPositions ?? true
            };

            var goal = await _goalAutomationAppService.CreateGoalAsync(input);
            return BuildActionResult(
                action.Type,
                goal.Status == GoalStatus.Rejected.ToString() ? "blocked" : "completed",
                goal.Status == GoalStatus.Rejected.ToString() ? "Goal rejected" : "Goal created",
                $"{goal.Name}: {goal.StatusReason}");
        }

        private async Task<AssistantActionResultDto> ListGoalTargetsAsync()
        {
            var goals = await _goalAutomationAppService.GetMyGoalsAsync();
            if (goals.Items == null || goals.Items.Count == 0)
            {
                return BuildActionResult("list_goal_targets", "completed", "No goals yet", "You do not have any BTC goal targets yet.");
            }

            var summary = string.Join(" | ", goals.Items.Take(3).Select(goal => $"{goal.Name} [{goal.Status}] {goal.ProgressPercent:0.##}%"));
            return BuildActionResult("list_goal_targets", "completed", "Goals loaded", summary);
        }

        private async Task<AssistantActionResultDto> PauseGoalTargetAsync(AssistantPlannedAction action)
        {
            var goal = await ResolveGoalAsync(action);
            if (goal == null)
            {
                return BuildActionResult(action.Type, "needs_input", "Goal not found", "Tell me which goal you want me to pause.");
            }

            var result = await _goalAutomationAppService.PauseGoalAsync(new EntityDto<long>(goal.Id));
            return BuildActionResult(action.Type, "completed", "Goal paused", $"{result.Name}: {result.StatusReason}");
        }

        private async Task<AssistantActionResultDto> CancelGoalTargetAsync(AssistantPlannedAction action)
        {
            var goal = await ResolveGoalAsync(action);
            if (goal == null)
            {
                return BuildActionResult(action.Type, "needs_input", "Goal not found", "Tell me which goal you want me to cancel.");
            }

            var result = await _goalAutomationAppService.CancelGoalAsync(new EntityDto<long>(goal.Id));
            return BuildActionResult(action.Type, "completed", "Goal canceled", $"{result.Name}: {result.StatusReason}");
        }

        private async Task<long?> ResolveDefaultConnectionIdAsync()
        {
            var connections = await _externalBrokerAppService.GetMyConnectionsAsync();
            return connections.Items?.Where(x => x.IsActive && x.Status == ExternalBrokerConnectionStatus.Connected).Select(x => (long?)x.Id).FirstOrDefault();
        }

        private static AssistantActionResultDto BuildActionResult(string actionType, string status, string title, string summary)
        {
            return new AssistantActionResultDto
            {
                ActionType = actionType,
                Status = status,
                Title = title,
                Summary = summary
            };
        }

        private static TradeDirection ParseDirection(string direction)
        {
            return direction?.Trim().ToLowerInvariant() == "sell" ? TradeDirection.Sell : TradeDirection.Buy;
        }

        private static string NormalizeAlertSymbol(string symbol)
        {
            return string.IsNullOrWhiteSpace(symbol) ? "BTCUSDT" : symbol.Trim().ToUpperInvariant();
        }

        private static string NormalizeLiveSymbol(string symbol)
        {
            var normalized = NormalizeAlertSymbol(symbol).Replace("/", string.Empty);
            return normalized switch
            {
                "BTCUSDT" => "BTCUSD",
                "BTC" => "BTCUSD",
                _ => normalized
            };
        }

        private async Task<GoalTargetDto> ResolveGoalAsync(AssistantPlannedAction action)
        {
            var goals = await _goalAutomationAppService.GetMyGoalsAsync();
            if (goals.Items == null || goals.Items.Count == 0)
            {
                return null;
            }

            if (action.GoalId.HasValue)
            {
                return goals.Items.FirstOrDefault(x => x.Id == action.GoalId.Value);
            }

            if (!string.IsNullOrWhiteSpace(action.GoalName))
            {
                var match = goals.Items.FirstOrDefault(x => string.Equals(x.Name, action.GoalName, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match;
                }

                return goals.Items.FirstOrDefault(x => x.Name?.IndexOf(action.GoalName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var activeGoals = goals.Items.Where(x => x.Status == GoalStatus.Active.ToString() || x.Status == GoalStatus.Accepted.ToString()).ToList();
            return activeGoals.Count == 1 ? activeGoals[0] : null;
        }

        private static GoalAccountType ParseGoalAccountType(string value)
        {
            return string.Equals(value, "external", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "externalbroker", StringComparison.OrdinalIgnoreCase)
                ? GoalAccountType.ExternalBroker
                : GoalAccountType.PaperTrading;
        }

        private static GoalTargetType ParseGoalTargetType(string value)
        {
            return string.Equals(value, "amount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "targetamount", StringComparison.OrdinalIgnoreCase)
                ? GoalTargetType.TargetAmount
                : GoalTargetType.PercentGrowth;
        }

        private static GoalTradingSession ParseGoalTradingSession(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "europe" => GoalTradingSession.Europe,
                "us" => GoalTradingSession.Us,
                "europeusoverlap" => GoalTradingSession.EuropeUsOverlap,
                "overlap" => GoalTradingSession.EuropeUsOverlap,
                _ => GoalTradingSession.AnyTime
            };
        }
    }
}
