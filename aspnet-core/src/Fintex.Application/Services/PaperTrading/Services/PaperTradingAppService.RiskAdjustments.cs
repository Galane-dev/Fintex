using Fintex.Investments.MarketData.Dto;
using System;
using System.Collections.Generic;

namespace Fintex.Investments.PaperTrading
{
    public partial class PaperTradingAppService
    {
        private static bool ShouldHoldRecommendation(MarketVerdictDto verdict)
        {
            return verdict == null ||
                verdict.Verdict == MarketVerdict.Hold ||
                !verdict.ConfidenceScore.HasValue ||
                verdict.ConfidenceScore.Value < 45m ||
                !verdict.TrendScore.HasValue ||
                Math.Abs(verdict.TrendScore.Value) < 15m;
        }

        private static void ApplyVerdictQualityAdjustments(
            MarketVerdictDto verdict,
            PaperTradeMarketContext marketContext,
            TradeDirection direction,
            decimal normalizedDirection,
            ICollection<string> reasons,
            ICollection<string> suggestions,
            ref decimal riskScore)
        {
            if (verdict == null)
            {
                riskScore += 32m;
                AddUnique(reasons, "The market verdict is still loading, so the setup cannot be confirmed yet.");
                AddUnique(suggestions, "Wait for the realtime verdict stack to finish loading before entering.");
                return;
            }

            var confidence = verdict.ConfidenceScore ?? 0m;
            var trend = verdict.TrendScore ?? 0m;
            var alignment = verdict.TimeframeAlignmentScore ?? 0m;
            var structureScore = (verdict.StructureScore ?? 0m) / 100m;

            ApplyConfidenceAdjustment(confidence, reasons, suggestions, ref riskScore);
            ApplyTrendAdjustment(verdict, direction, normalizedDirection, trend, reasons, suggestions, ref riskScore);
            ApplyAlignmentAdjustment(alignment, normalizedDirection, reasons, ref riskScore);
            ApplyAdxAdjustment(verdict.Adx, reasons, ref riskScore);
            ApplyAtrAdjustment(verdict.AtrPercent, reasons, suggestions, ref riskScore);
            ApplySpreadAdjustment(marketContext.SpreadPercent, reasons, ref riskScore);
            ApplyRsiAdjustment(verdict.Rsi, direction, reasons, ref riskScore);
            ApplyMacdAdjustment(verdict.MacdHistogram, normalizedDirection, reasons, ref riskScore);
            ApplyStructureAdjustment(verdict.StructureLabel, structureScore, normalizedDirection, reasons, ref riskScore);
        }

        private static void ApplyConfidenceAdjustment(
            decimal confidence,
            ICollection<string> reasons,
            ICollection<string> suggestions,
            ref decimal riskScore)
        {
            if (confidence < 45m)
            {
                riskScore += 20m;
                AddUnique(reasons, "Confidence is still weak, so the market-only read does not have much conviction yet.");
                AddUnique(suggestions, "Wait for confidence to build before pressing the trade.");
            }
            else if (confidence < 60m)
            {
                riskScore += 8m;
                AddUnique(reasons, "Confidence is only moderate, so the setup still needs discipline.");
            }
            else
            {
                riskScore -= 6m;
                AddUnique(reasons, "Confidence is healthy enough to support a structured attempt.");
            }
        }

        private static void ApplyTrendAdjustment(
            MarketVerdictDto verdict,
            TradeDirection direction,
            decimal normalizedDirection,
            decimal trend,
            ICollection<string> reasons,
            ICollection<string> suggestions,
            ref decimal riskScore)
        {
            if (Math.Abs(trend) < 15m)
            {
                riskScore += 12m;
                AddUnique(reasons, "Trend score is still shallow, so there is not much directional edge yet.");
            }
            else if ((trend * normalizedDirection) < 0m)
            {
                riskScore += 30m;
                AddUnique(reasons, $"This {direction.ToString().ToLowerInvariant()} goes against the current {verdict.Verdict.ToString().ToLowerInvariant()} bias.");
                AddUnique(suggestions, $"Only {direction.ToString().ToLowerInvariant()} once trend and alignment turn in your favor.");
            }
            else
            {
                riskScore -= 12m;
                AddUnique(reasons, $"This {direction.ToString().ToLowerInvariant()} is aligned with the current {verdict.Verdict.ToString().ToLowerInvariant()} bias.");
            }
        }

        private static void ApplyAlignmentAdjustment(decimal alignment, decimal normalizedDirection, ICollection<string> reasons, ref decimal riskScore)
        {
            if (Math.Abs(alignment) < 10m)
            {
                riskScore += 9m;
                AddUnique(reasons, "Higher timeframes are not aligned strongly enough yet.");
            }
            else if ((alignment * normalizedDirection) < 0m)
            {
                riskScore += 16m;
                AddUnique(reasons, "5m, 15m, and 1h confirmation are leaning against this trade.");
            }
            else
            {
                riskScore -= 6m;
                AddUnique(reasons, "Multi-timeframe confirmation supports the side you want to take.");
            }
        }

        private static void ApplyAdxAdjustment(decimal? adx, ICollection<string> reasons, ref decimal riskScore)
        {
            if (!adx.HasValue)
            {
                return;
            }

            if (adx.Value < 15m)
            {
                riskScore += 12m;
                AddUnique(reasons, "ADX is weak, which means the move may not have trend strength behind it.");
            }
            else if (adx.Value < 25m)
            {
                riskScore += 5m;
                AddUnique(reasons, "ADX is only middling, so trend continuation is not fully convincing.");
            }
            else
            {
                riskScore -= 4m;
                AddUnique(reasons, "ADX confirms that the market has usable directional strength.");
            }
        }

        private static void ApplyAtrAdjustment(decimal? atrPercent, ICollection<string> reasons, ICollection<string> suggestions, ref decimal riskScore)
        {
            if (!atrPercent.HasValue)
            {
                return;
            }

            if (atrPercent.Value >= 0.85m)
            {
                riskScore += 12m;
                AddUnique(reasons, "ATR is elevated, so price can whip through weak setups quickly.");
                AddUnique(suggestions, "Use tighter confirmation and avoid oversized exposure while ATR stays hot.");
            }
            else if (atrPercent.Value >= 0.60m)
            {
                riskScore += 6m;
                AddUnique(reasons, "ATR is above calm conditions, so execution needs more care.");
            }
            else
            {
                riskScore -= 2m;
            }
        }

        private static void ApplySpreadAdjustment(decimal? spreadPercent, ICollection<string> reasons, ref decimal riskScore)
        {
            if (spreadPercent.HasValue && spreadPercent.Value >= 0.05m)
            {
                riskScore += 7m;
                AddUnique(reasons, "Spread is wider than ideal, which raises execution friction.");
            }
        }

        private static void ApplyRsiAdjustment(decimal? rsi, TradeDirection direction, ICollection<string> reasons, ref decimal riskScore)
        {
            if (!rsi.HasValue)
            {
                return;
            }

            if (direction == TradeDirection.Buy)
            {
                if (rsi.Value >= 72m)
                {
                    riskScore += 16m;
                    AddUnique(reasons, "RSI is already stretched high, so chasing the buy increases reversal risk.");
                }
                else if (rsi.Value <= 35m)
                {
                    riskScore -= 5m;
                    AddUnique(reasons, "RSI is compressed enough to support a mean-reversion style buy.");
                }

                return;
            }

            if (rsi.Value <= 28m)
            {
                riskScore += 16m;
                AddUnique(reasons, "RSI is already deeply compressed, so pressing a sell here risks chasing the move late.");
            }
            else if (rsi.Value >= 65m)
            {
                riskScore -= 5m;
                AddUnique(reasons, "RSI is elevated enough to support a fade or bearish reversal idea.");
            }
        }

        private static void ApplyMacdAdjustment(decimal? macdHistogram, decimal normalizedDirection, ICollection<string> reasons, ref decimal riskScore)
        {
            if (!macdHistogram.HasValue)
            {
                return;
            }

            if ((macdHistogram.Value * normalizedDirection) < 0m)
            {
                riskScore += 10m;
                AddUnique(reasons, "MACD histogram is pushing against this trade direction.");
            }
            else
            {
                riskScore -= 4m;
            }
        }

        private static void ApplyStructureAdjustment(
            string structureLabel,
            decimal structureScore,
            decimal normalizedDirection,
            ICollection<string> reasons,
            ref decimal riskScore)
        {
            if ((structureScore * normalizedDirection) < -0.15m)
            {
                riskScore += 12m;
                AddUnique(reasons, $"Market structure currently looks better for the opposite side: {structureLabel?.ToLowerInvariant()}.");
            }
            else if ((structureScore * normalizedDirection) > 0.15m)
            {
                riskScore -= 6m;
                AddUnique(reasons, $"Structure supports this direction: {structureLabel?.ToLowerInvariant()}.");
            }
        }

    }
}
