using Fintex.Investments.MarketData.Dto;
using Fintex.Investments.EconomicCalendar;
using Fintex.Investments.News;
using Fintex.Investments.PaperTrading.Dto;
using System;
using System.Collections.Generic;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        private static SuggestedTradePlan BuildSuggestedTradePlan(decimal entryPrice, decimal? atr, TradeDirection direction)
        {
            var volatilityDistance = atr.HasValue && atr.Value > 0m
                ? Math.Max(atr.Value * 0.90m, entryPrice * 0.0025m)
                : entryPrice * 0.0035m;
            var rewardDistance = volatilityDistance * 1.80m;

            var stopLoss = direction == TradeDirection.Buy ? entryPrice - volatilityDistance : entryPrice + volatilityDistance;
            var takeProfit = direction == TradeDirection.Buy ? entryPrice + rewardDistance : entryPrice - rewardDistance;

            return new SuggestedTradePlan
            {
                StopLoss = decimal.Round(stopLoss, 2, MidpointRounding.AwayFromZero),
                TakeProfit = decimal.Round(takeProfit, 2, MidpointRounding.AwayFromZero),
                RewardRiskRatio = decimal.Round(rewardDistance / Math.Max(volatilityDistance, 0.00000001m), 2, MidpointRounding.AwayFromZero)
            };
        }

        private static decimal? CalculateRewardRiskRatio(
            TradeDirection direction,
            decimal entryPrice,
            decimal? stopLoss,
            decimal? takeProfit)
        {
            if (!stopLoss.HasValue || !takeProfit.HasValue)
            {
                return null;
            }

            decimal riskDistance;
            decimal rewardDistance;

            if (direction == TradeDirection.Buy)
            {
                riskDistance = entryPrice - stopLoss.Value;
                rewardDistance = takeProfit.Value - entryPrice;
            }
            else
            {
                riskDistance = stopLoss.Value - entryPrice;
                rewardDistance = entryPrice - takeProfit.Value;
            }

            if (riskDistance <= 0m || rewardDistance <= 0m)
            {
                return null;
            }

            return decimal.Round(rewardDistance / riskDistance, 2, MidpointRounding.AwayFromZero);
        }

        private static bool IsStopLossValid(TradeDirection direction, decimal entryPrice, decimal stopLoss)
        {
            return direction == TradeDirection.Buy ? stopLoss < entryPrice : stopLoss > entryPrice;
        }

        private static bool IsTakeProfitValid(TradeDirection direction, decimal entryPrice, decimal takeProfit)
        {
            return direction == TradeDirection.Buy ? takeProfit > entryPrice : takeProfit < entryPrice;
        }

        private static List<string> BuildHoldReasons(MarketVerdictDto verdict)
        {
            var reasons = new List<string>();

            if (verdict == null)
            {
                reasons.Add("The realtime verdict stack is still loading.");
                return reasons;
            }

            if (verdict.ConfidenceScore.HasValue && verdict.ConfidenceScore.Value < 45m)
            {
                reasons.Add("Confidence is too low to justify forcing a trade.");
            }

            if (verdict.TrendScore.HasValue && Math.Abs(verdict.TrendScore.Value) < 15m)
            {
                reasons.Add("Trend score is still shallow, so the edge is weak.");
            }

            if (verdict.TimeframeAlignmentScore.HasValue && Math.Abs(verdict.TimeframeAlignmentScore.Value) < 10m)
            {
                reasons.Add("Higher timeframes are not aligned strongly enough.");
            }

            if (reasons.Count == 0)
            {
                reasons.Add("The current market-only read still favors patience over action.");
            }

            return reasons;
        }

        private static void ApplyNewsOverlay(PaperTradeRecommendationDto recommendation, NewsRecommendationInsight newsInsight)
        {
            if (recommendation == null || newsInsight == null)
            {
                return;
            }

            recommendation.NewsSummary = newsInsight.Summary;
            recommendation.NewsImpactScore = newsInsight.ImpactScore;
            recommendation.NewsSentiment = newsInsight.Sentiment.ToString();
            recommendation.NewsRecommendedAction = newsInsight.RecommendedAction;
            recommendation.NewsLastUpdatedAt = newsInsight.GeneratedAt;
            recommendation.NewsHeadlines = newsInsight.KeyHeadlines ?? new List<string>();

            if (newsInsight.ImpactScore >= 55m)
            {
                AddUnique(recommendation.Reasons, "Recent Bitcoin and US Dollar headlines are active enough to matter for the current setup.");
            }

            if (newsInsight.ImpactScore >= 75m &&
                recommendation.RecommendedAction != MarketVerdict.Hold &&
                newsInsight.RecommendedAction != MarketVerdict.Hold &&
                newsInsight.RecommendedAction != recommendation.RecommendedAction)
            {
                recommendation.RiskScore = decimal.Round(Clamp(recommendation.RiskScore + 10m, 0m, 100m), 2, MidpointRounding.AwayFromZero);
                recommendation.RiskLevel = GetRiskLevel(recommendation.RiskScore);
                recommendation.RecommendedAction = MarketVerdict.Hold;
                recommendation.Headline = "News and technicals are conflicting, so patience is safer right now.";
                recommendation.Summary = "The technical setup has an edge, but the latest Bitcoin or US Dollar headlines lean the other way strongly enough that the cleaner move is to wait.";
                AddUnique(recommendation.Reasons, $"News flow currently leans {newsInsight.RecommendedAction.ToString().ToLowerInvariant()}, which conflicts with the technical read.");
                AddUnique(recommendation.Suggestions, "Wait for price structure and the headline backdrop to align before committing.");
                return;
            }

            if (newsInsight.ImpactScore >= 55m &&
                recommendation.RecommendedAction != MarketVerdict.Hold &&
                newsInsight.RecommendedAction == recommendation.RecommendedAction)
            {
                recommendation.RiskScore = decimal.Round(Clamp(recommendation.RiskScore - 4m, 0m, 100m), 2, MidpointRounding.AwayFromZero);
                recommendation.RiskLevel = GetRiskLevel(recommendation.RiskScore);
                AddUnique(recommendation.Reasons, $"Headline flow also leans {recommendation.RecommendedAction.ToString().ToLowerInvariant()}, which supports the trade direction.");
            }

            if (newsInsight.ImpactScore >= 65m)
            {
                AddUnique(recommendation.Suggestions, "Size more carefully around high-impact headlines, even when the setup looks good.");
            }
        }

        private static void ApplyEconomicCalendarOverlay(PaperTradeRecommendationDto recommendation, EconomicCalendarInsight economicCalendarInsight)
        {
            if (recommendation == null || economicCalendarInsight == null)
            {
                return;
            }

            recommendation.EconomicCalendarSummary = economicCalendarInsight.Summary;
            recommendation.EconomicCalendarRiskScore = economicCalendarInsight.RiskScore;
            recommendation.EconomicCalendarNextEventAtUtc = economicCalendarInsight.NextEventAtUtc;
            recommendation.EconomicCalendarEvents = economicCalendarInsight.UpcomingEvents?
                .ConvertAll(item => new EconomicCalendarEventDto
                {
                    Title = item.Title,
                    Source = item.Source,
                    OccursAtUtc = item.OccursAtUtc,
                    ImpactScore = item.ImpactScore
                }) ?? new List<EconomicCalendarEventDto>();

            if (economicCalendarInsight.RiskScore <= 0m)
            {
                return;
            }

            AddUnique(recommendation.Reasons, "Upcoming macro-event risk is active enough to matter for this setup.");

            if (economicCalendarInsight.RiskScore >= 55m)
            {
                recommendation.RiskScore = decimal.Round(Clamp(recommendation.RiskScore + 8m, 0m, 100m), 2, MidpointRounding.AwayFromZero);
                recommendation.RiskLevel = GetRiskLevel(recommendation.RiskScore);
                AddUnique(recommendation.Suggestions, "Reduce size or wait for the macro release window to pass before committing.");
            }

            if (economicCalendarInsight.RiskScore >= 75m && recommendation.RecommendedAction != MarketVerdict.Hold)
            {
                recommendation.RiskScore = decimal.Round(Clamp(recommendation.RiskScore + 10m, 0m, 100m), 2, MidpointRounding.AwayFromZero);
                recommendation.RiskLevel = GetRiskLevel(recommendation.RiskScore);
                recommendation.RecommendedAction = MarketVerdict.Hold;
                recommendation.Headline = "Macro-event risk is too close, so patience is safer right now.";
                recommendation.Summary = "A high-impact CPI, NFP, or FOMC release is too close to justify forcing a fresh trade before the event clears.";
                AddUnique(recommendation.Reasons, economicCalendarInsight.Summary);
            }
        }

        private static void AddUnique(ICollection<string> collection, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || collection.Contains(value))
            {
                return;
            }

            collection.Add(value);
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static PaperTradeRiskLevel GetRiskLevel(decimal riskScore)
        {
            return riskScore >= 72m
                ? PaperTradeRiskLevel.High
                : riskScore >= 45m
                    ? PaperTradeRiskLevel.Medium
                    : PaperTradeRiskLevel.Low;
        }
    }
}
