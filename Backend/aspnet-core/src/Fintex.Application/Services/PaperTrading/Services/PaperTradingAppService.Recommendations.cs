using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.EconomicCalendar;
using Fintex.Investments.News;
using Fintex.Investments.PaperTrading.Dto;
using Abp.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        public async Task<PaperTradeRecommendationDto> GetRecommendationAsync(GetPaperTradeRecommendationInput input)
        {
            var userId = AbpSession.GetUserId();
            var account = await _paperTradingAccountRepository.GetActiveForUserAsync(userId);
            var marketContext = await GetMarketContextAsync(input.Symbol, input.Provider);
            var newsInsight = await _newsRecommendationService.GetBitcoinUsdInsightAsync(
                marketContext.RealtimeVerdict,
                CancellationToken.None);
            var economicCalendarInsight = await _economicCalendarService.GetBitcoinUsdRiskInsightAsync(CancellationToken.None);
            var suggestedPlan = BuildSuggestedTradePlan(
                marketContext.LatestPoint.Price,
                marketContext.RealtimeVerdict?.Atr,
                marketContext.RealtimeVerdict?.Verdict == MarketVerdict.Sell ? TradeDirection.Sell : TradeDirection.Buy);
            var suggestedTradeAction = marketContext.RealtimeVerdict?.Verdict == MarketVerdict.Buy ||
                marketContext.RealtimeVerdict?.Verdict == MarketVerdict.Sell
                    ? marketContext.RealtimeVerdict.Verdict
                    : (MarketVerdict?)null;

            if (_recommendationGuardService.ShouldHold(marketContext.RealtimeVerdict))
            {
                var holdRecommendation = new PaperTradeRecommendationDto
                {
                    RecommendedAction = MarketVerdict.Hold,
                    SuggestedTradeAction = null,
                    RiskScore = 82m,
                    RiskLevel = PaperTradeRiskLevel.High,
                    Headline = "Best move is to wait for a cleaner setup.",
                    Summary = "The market-only read is not aligned strongly enough yet, so forcing a paper trade here would be closer to a gamble than a structured setup.",
                    ReferencePrice = marketContext.LatestPoint.Price,
                    Spread = marketContext.Spread,
                    SpreadPercent = marketContext.SpreadPercent,
                    ConfidenceScore = marketContext.RealtimeVerdict?.ConfidenceScore,
                    TrendScore = marketContext.RealtimeVerdict?.TrendScore,
                    Reasons = BuildHoldReasons(marketContext.RealtimeVerdict),
                    Suggestions = new List<string>
                    {
                        "Wait for 5m, 15m, and 1h alignment to strengthen before entering.",
                        "Look for ADX to firm up and for structure to break more decisively in one direction.",
                        "Only size in once stop loss and take profit levels are defined before the click."
                    }
                };

                ApplyNewsOverlay(holdRecommendation, newsInsight);
                ApplyEconomicCalendarOverlay(holdRecommendation, economicCalendarInsight);
                Logger.Debug($"Recommendation outcome for {input.Symbol}: hold, verdictState={marketContext.RealtimeVerdict?.VerdictState}, confidence={marketContext.RealtimeVerdict?.ConfidenceScore}, trend={marketContext.RealtimeVerdict?.TrendScore}, price={marketContext.LatestPoint.Price}.");
                return holdRecommendation;
            }

            var recommendedDirection = marketContext.RealtimeVerdict.Verdict == MarketVerdict.Buy
                ? TradeDirection.Buy
                : TradeDirection.Sell;
            var assessment = BuildTradeAssessment(
                account,
                marketContext,
                recommendedDirection,
                input.Quantity,
                input.StopLoss,
                input.TakeProfit);

            var recommendation = new PaperTradeRecommendationDto
            {
                RecommendedAction = marketContext.RealtimeVerdict.Verdict,
                SuggestedTradeAction = suggestedTradeAction,
                RiskScore = assessment.RiskScore,
                RiskLevel = assessment.RiskLevel,
                Headline = marketContext.RealtimeVerdict.Verdict == MarketVerdict.Buy
                    ? "Current edge favors a buy setup."
                    : "Current edge favors a sell setup.",
                Summary = assessment.RiskLevel == PaperTradeRiskLevel.High
                    ? "The market leans in one direction, but your current trade plan still needs work before it becomes disciplined."
                    : "This is the cleaner side of the market right now, provided you keep the setup disciplined.",
                ReferencePrice = marketContext.LatestPoint.Price,
                Spread = marketContext.Spread,
                SpreadPercent = marketContext.SpreadPercent,
                SuggestedStopLoss = assessment.SuggestedStopLoss ?? suggestedPlan.StopLoss,
                SuggestedTakeProfit = assessment.SuggestedTakeProfit ?? suggestedPlan.TakeProfit,
                ConfidenceScore = assessment.ConfidenceScore,
                TrendScore = assessment.TrendScore,
                Reasons = assessment.Reasons,
                Suggestions = assessment.Suggestions
            };

            ApplyNewsOverlay(recommendation, newsInsight);
            ApplyEconomicCalendarOverlay(recommendation, economicCalendarInsight);
            Logger.Debug($"Recommendation outcome for {input.Symbol}: {recommendation.RecommendedAction}, risk={recommendation.RiskScore}, confidence={recommendation.ConfidenceScore}, trend={recommendation.TrendScore}, price={recommendation.ReferencePrice}.");
            return recommendation;
        }

        private PaperTradeAssessmentDto BuildTradeAssessment(
            PaperTradingAccount account,
            PaperTradeMarketContext marketContext,
            TradeDirection direction,
            decimal? quantity,
            decimal? stopLoss,
            decimal? takeProfit)
        {
            var reasons = new List<string>();
            var suggestions = new List<string>();
            var latestPrice = marketContext.LatestPoint.Price;
            var verdict = marketContext.RealtimeVerdict;
            var suggestedPlan = BuildSuggestedTradePlan(latestPrice, verdict?.Atr, direction);
            var effectiveQuantity = quantity.GetValueOrDefault();
            var normalizedDirection = direction == TradeDirection.Buy ? 1m : -1m;
            var riskScore = 34m;

            ApplyVerdictQualityAdjustments(verdict, marketContext, direction, normalizedDirection, reasons, suggestions, ref riskScore);
            ApplyTicketQualityAdjustments(latestPrice, direction, stopLoss, takeProfit, suggestedPlan, reasons, suggestions, ref riskScore);
            var effectiveStopLoss = stopLoss ?? suggestedPlan.StopLoss;
            var effectiveTakeProfit = takeProfit ?? suggestedPlan.TakeProfit;
            var rewardRiskRatio = CalculateRewardRiskRatio(direction, latestPrice, effectiveStopLoss, effectiveTakeProfit);
            ApplyRewardRiskAdjustment(rewardRiskRatio, reasons, ref riskScore);
            ApplySizingAdjustment(account, latestPrice, effectiveQuantity, reasons, suggestions, ref riskScore);

            riskScore = decimal.Round(Clamp(riskScore, 0m, 100m), 2, MidpointRounding.AwayFromZero);
            var riskLevel = GetRiskLevel(riskScore);

            return new PaperTradeAssessmentDto
            {
                Direction = direction,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                ShouldBlock = riskLevel == PaperTradeRiskLevel.High,
                Headline = riskLevel == PaperTradeRiskLevel.High
                    ? "Trade blocked: the setup is too risky right now."
                    : riskLevel == PaperTradeRiskLevel.Medium
                        ? "Trade placed, but this one still looks like a gamble."
                        : "Trade placed: this is a cleaner, more disciplined setup.",
                Summary = riskLevel == PaperTradeRiskLevel.High
                    ? "The current plan breaks too many quality checks, so the simulator stopped the trade before execution."
                    : riskLevel == PaperTradeRiskLevel.Medium
                        ? "The trade was allowed, but the setup still needs better structure or risk definition if you want better odds."
                        : "The market bias, structure, and risk plan are aligned well enough to support the trade. Keep repeating setups like this.",
                ReferencePrice = latestPrice,
                Spread = marketContext.Spread,
                SpreadPercent = marketContext.SpreadPercent,
                SuggestedStopLoss = suggestedPlan.StopLoss,
                SuggestedTakeProfit = suggestedPlan.TakeProfit,
                SuggestedRewardRiskRatio = rewardRiskRatio ?? suggestedPlan.RewardRiskRatio,
                ConfidenceScore = verdict?.ConfidenceScore,
                TrendScore = verdict?.TrendScore,
                TimeframeAlignmentScore = verdict?.TimeframeAlignmentScore,
                StructureLabel = verdict?.StructureLabel,
                MarketVerdict = verdict?.Verdict ?? MarketVerdict.Hold,
                Reasons = reasons.Take(5).ToList(),
                Suggestions = suggestions.Take(5).ToList()
            };
        }

    }
}
