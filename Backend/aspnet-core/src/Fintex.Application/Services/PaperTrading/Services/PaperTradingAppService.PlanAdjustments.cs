using System.Collections.Generic;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        private static void ApplyTicketQualityAdjustments(
            decimal latestPrice,
            TradeDirection direction,
            decimal? stopLoss,
            decimal? takeProfit,
            SuggestedTradePlan suggestedPlan,
            ICollection<string> reasons,
            ICollection<string> suggestions,
            ref decimal riskScore)
        {
            if (!stopLoss.HasValue)
            {
                riskScore += 18m;
                AddUnique(reasons, "There is no stop loss on the ticket yet.");
                AddUnique(suggestions, $"Use a stop loss near {suggestedPlan.StopLoss:N2} so the downside stays defined.");
            }
            else if (!IsStopLossValid(direction, latestPrice, stopLoss.Value))
            {
                riskScore += 25m;
                AddUnique(reasons, "The stop loss is on the wrong side of the entry, so the plan is structurally invalid.");
                AddUnique(suggestions, $"Move the stop loss to the correct side of the entry, around {suggestedPlan.StopLoss:N2}.");
            }

            if (!takeProfit.HasValue)
            {
                riskScore += 8m;
                AddUnique(reasons, "There is no take profit on the ticket yet.");
                AddUnique(suggestions, $"Set a take profit near {suggestedPlan.TakeProfit:N2} so the trade has a defined target.");
            }
            else if (!IsTakeProfitValid(direction, latestPrice, takeProfit.Value))
            {
                riskScore += 18m;
                AddUnique(reasons, "The take profit is on the wrong side of the entry.");
                AddUnique(suggestions, $"Move the take profit to the correct side of the entry, around {suggestedPlan.TakeProfit:N2}.");
            }
        }

        private static void ApplyRewardRiskAdjustment(decimal? rewardRiskRatio, ICollection<string> reasons, ref decimal riskScore)
        {
            if (!rewardRiskRatio.HasValue)
            {
                return;
            }

            if (rewardRiskRatio.Value < 1m)
            {
                riskScore += 15m;
                AddUnique(reasons, "Reward-to-risk is below 1:1, which makes the setup poor even if the direction is right.");
            }
            else if (rewardRiskRatio.Value < 1.5m)
            {
                riskScore += 7m;
                AddUnique(reasons, "Reward-to-risk is only modest, so the setup needs strong accuracy to pay well.");
            }
            else if (rewardRiskRatio.Value >= 2m)
            {
                riskScore -= 6m;
                AddUnique(reasons, "Reward-to-risk is healthy for a disciplined paper trade.");
            }
        }

        private static void ApplySizingAdjustment(
            PaperTradingAccount account,
            decimal latestPrice,
            decimal effectiveQuantity,
            ICollection<string> reasons,
            ICollection<string> suggestions,
            ref decimal riskScore)
        {
            if (account == null || effectiveQuantity <= 0m || account.Equity <= 0m)
            {
                return;
            }

            var notional = effectiveQuantity * latestPrice;
            var exposurePercent = (notional / account.Equity) * 100m;

            if (exposurePercent >= 80m)
            {
                riskScore += 22m;
                AddUnique(reasons, "The position size is oversized relative to current equity.");
                AddUnique(suggestions, "Reduce size so one idea does not dominate the whole paper account.");
            }
            else if (exposurePercent >= 40m)
            {
                riskScore += 10m;
                AddUnique(reasons, "The position size is aggressive relative to current equity.");
            }
            else if (exposurePercent <= 15m)
            {
                riskScore -= 3m;
            }
        }
    }
}
