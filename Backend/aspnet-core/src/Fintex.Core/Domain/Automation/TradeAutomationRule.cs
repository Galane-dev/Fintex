using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Fintex.Investments.Notifications;
using System;

namespace Fintex.Investments.Automation
{
    /// <summary>
    /// User-owned rule that automatically executes a trade when a market trigger is satisfied.
    /// </summary>
    public class TradeAutomationRule : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxNameLength = 128;
        public const int MaxSymbolLength = 32;
        public const int MaxNotesLength = 1024;

        protected TradeAutomationRule()
        {
        }

        public TradeAutomationRule(
            int? tenantId,
            long userId,
            string name,
            string symbol,
            MarketDataProvider provider,
            TradeAutomationTriggerType triggerType,
            decimal? createdMetricValue,
            decimal? targetMetricValue,
            MarketVerdict? targetVerdict,
            decimal? minimumConfidenceScore,
            TradeAutomationDestination destination,
            long? externalConnectionId,
            TradeDirection tradeDirection,
            decimal quantity,
            decimal? stopLoss,
            decimal? takeProfit,
            bool notifyInApp,
            bool notifyEmail,
            string notes)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User id must be greater than zero.");
            }

            if (quantity <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            if (destination == TradeAutomationDestination.ExternalBroker && !externalConnectionId.HasValue)
            {
                throw new ArgumentException("An external broker connection is required for broker automation.");
            }

            TenantId = tenantId;
            UserId = userId;
            Name = LimitRequired(name, MaxNameLength, "Rule name is required.");
            Symbol = LimitRequired(symbol, MaxSymbolLength, "Rule symbol is required.").ToUpperInvariant();
            Provider = provider;
            TriggerType = triggerType;
            CreatedMetricValue = NormalizeNullable(createdMetricValue);
            LastObservedMetricValue = CreatedMetricValue;
            TargetMetricValue = NormalizeNullable(targetMetricValue);
            TargetVerdict = triggerType == TradeAutomationTriggerType.Verdict ? targetVerdict : null;
            MinimumConfidenceScore = NormalizeNullable(minimumConfidenceScore);
            Destination = destination;
            ExternalConnectionId = externalConnectionId;
            TradeDirection = tradeDirection;
            Quantity = NormalizeRequired(quantity);
            StopLoss = NormalizeNullable(stopLoss);
            TakeProfit = NormalizeNullable(takeProfit);
            NotifyInApp = notifyInApp;
            NotifyEmail = notifyEmail;
            Notes = LimitOptional(notes, MaxNotesLength);
            IsActive = true;

            ValidateTriggerConfiguration();
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string Name { get; protected set; }

        public string Symbol { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public TradeAutomationTriggerType TriggerType { get; protected set; }

        public decimal? CreatedMetricValue { get; protected set; }

        public decimal? LastObservedMetricValue { get; protected set; }

        public decimal? TargetMetricValue { get; protected set; }

        public MarketVerdict? TargetVerdict { get; protected set; }

        public decimal? MinimumConfidenceScore { get; protected set; }

        public TradeAutomationDestination Destination { get; protected set; }

        public long? ExternalConnectionId { get; protected set; }

        public TradeDirection TradeDirection { get; protected set; }

        public decimal Quantity { get; protected set; }

        public decimal? StopLoss { get; protected set; }

        public decimal? TakeProfit { get; protected set; }

        public bool NotifyInApp { get; protected set; }

        public bool NotifyEmail { get; protected set; }

        public bool IsActive { get; protected set; }

        public DateTime? LastTriggeredAt { get; protected set; }

        public long? LastTradeId { get; protected set; }

        public long? LastNotificationId { get; protected set; }

        public string Notes { get; protected set; }

        public bool ShouldTrigger(NotificationMarketSnapshot snapshot)
        {
            if (!IsActive || snapshot == null)
            {
                return false;
            }

            if (TriggerType == TradeAutomationTriggerType.Verdict)
            {
                return snapshot.Verdict == TargetVerdict &&
                    (!MinimumConfidenceScore.HasValue ||
                        (snapshot.ConfidenceScore.HasValue && snapshot.ConfidenceScore.Value >= MinimumConfidenceScore.Value));
            }

            var currentMetric = GetMetricValue(snapshot);
            if (!currentMetric.HasValue || !TargetMetricValue.HasValue)
            {
                return false;
            }

            if (TriggerType == TradeAutomationTriggerType.PriceTarget)
            {
                return HasPriceCrossed(
                    LastObservedMetricValue ?? CreatedMetricValue ?? currentMetric.Value,
                    currentMetric.Value,
                    snapshot.Bid,
                    snapshot.Ask,
                    TargetMetricValue.Value);
            }

            return HasCrossed(
                LastObservedMetricValue ?? CreatedMetricValue ?? currentMetric.Value,
                currentMetric.Value,
                TargetMetricValue.Value);
        }

        public bool RefreshObservedMetric(NotificationMarketSnapshot snapshot)
        {
            var currentMetric = GetMetricValue(snapshot);
            if (!currentMetric.HasValue)
            {
                return false;
            }

            if (LastObservedMetricValue.HasValue && LastObservedMetricValue.Value == currentMetric.Value)
            {
                return false;
            }

            LastObservedMetricValue = currentMetric.Value;
            return true;
        }

        public void Trigger(long? notificationId, long? tradeId, DateTime occurredAt, decimal? currentMetricValue)
        {
            LastNotificationId = notificationId;
            LastTradeId = tradeId;
            LastTriggeredAt = occurredAt;
            LastObservedMetricValue = NormalizeNullable(currentMetricValue);
            IsActive = false;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public decimal? GetMetricValue(NotificationMarketSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }

            return TriggerType switch
            {
                TradeAutomationTriggerType.PriceTarget => NormalizeNullable(snapshot.Price),
                TradeAutomationTriggerType.RelativeStrengthIndex => NormalizeNullable(snapshot.Rsi),
                TradeAutomationTriggerType.MacdHistogram => NormalizeNullable(snapshot.MacdHistogram),
                TradeAutomationTriggerType.Momentum => NormalizeNullable(snapshot.Momentum),
                TradeAutomationTriggerType.TrendScore => NormalizeNullable(snapshot.TrendScore),
                TradeAutomationTriggerType.ConfidenceScore => NormalizeNullable(snapshot.ConfidenceScore),
                _ => null
            };
        }

        private void ValidateTriggerConfiguration()
        {
            if (TriggerType == TradeAutomationTriggerType.Verdict)
            {
                if (!TargetVerdict.HasValue || TargetVerdict == MarketVerdict.Hold)
                {
                    throw new ArgumentException("A buy or sell verdict target is required.");
                }

                return;
            }

            if (!TargetMetricValue.HasValue)
            {
                throw new ArgumentException("A trigger value is required for this automation rule.");
            }
        }

        private static bool HasCrossed(decimal previousValue, decimal currentValue, decimal targetValue)
        {
            return previousValue < targetValue && currentValue >= targetValue
                || previousValue > targetValue && currentValue <= targetValue;
        }

        private static bool HasPriceCrossed(
            decimal previousPrice,
            decimal currentPrice,
            decimal? bidPrice,
            decimal? askPrice,
            decimal targetPrice)
        {
            var currentLow = GetCurrentEdge(currentPrice, bidPrice, askPrice, useMinimum: true);
            var currentHigh = GetCurrentEdge(currentPrice, bidPrice, askPrice, useMinimum: false);
            return previousPrice < targetPrice && currentHigh >= targetPrice
                || previousPrice > targetPrice && currentLow <= targetPrice;
        }

        private static decimal GetCurrentEdge(decimal currentPrice, decimal? bidPrice, decimal? askPrice, bool useMinimum)
        {
            var roundedCurrent = NormalizeRequired(currentPrice);
            var roundedBid = NormalizeNullable(bidPrice);
            var roundedAsk = NormalizeNullable(askPrice);

            var low = roundedBid.HasValue && roundedBid.Value > 0m && roundedBid.Value < roundedCurrent
                ? roundedBid.Value
                : roundedCurrent;
            var high = roundedAsk.HasValue && roundedAsk.Value > 0m && roundedAsk.Value > roundedCurrent
                ? roundedAsk.Value
                : roundedCurrent;

            return useMinimum ? low : high;
        }

        private static decimal NormalizeRequired(decimal value)
        {
            return decimal.Round(value, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal? NormalizeNullable(decimal? value)
        {
            return value.HasValue
                ? NormalizeRequired(value.Value)
                : null;
        }

        private static string LimitRequired(string value, int maxLength, string error)
        {
            var limited = LimitOptional(value, maxLength);
            if (string.IsNullOrWhiteSpace(limited))
            {
                throw new ArgumentException(error);
            }

            return limited;
        }

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
