using Fintex.Investments.Assistant.Dto;
using Fintex.Investments.Brokers.Dto;
using Fintex.Investments.Notifications.Dto;
using Fintex.Investments.PaperTrading.Dto;
using System.Collections.Generic;
using System.Linq;
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
    }
}
