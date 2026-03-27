using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Linq;

namespace Fintex.Investments.Notifications
{
    /// <summary>
    /// User-owned rule that triggers when the market crosses a configured price level.
    /// </summary>
    public class NotificationAlertRule : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxNameLength = 128;
        public const int MaxSymbolLength = 32;
        public const int MaxNotesLength = 1024;

        protected NotificationAlertRule()
        {
        }

        public NotificationAlertRule(
            int? tenantId,
            long userId,
            string name,
            string symbol,
            MarketDataProvider provider,
            decimal createdPrice,
            decimal targetPrice,
            bool notifyInApp,
            bool notifyEmail,
            string notes)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User id must be greater than zero.");
            }

            if (createdPrice <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(createdPrice), "Created price must be greater than zero.");
            }

            if (targetPrice <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPrice), "Target price must be greater than zero.");
            }

            TenantId = tenantId;
            UserId = userId;
            Name = LimitRequired(name, MaxNameLength, "Alert name is required.");
            Symbol = LimitRequired(symbol, MaxSymbolLength, "Alert symbol is required.").ToUpperInvariant();
            Provider = provider;
            AlertType = NotificationAlertRuleType.PriceTarget;
            CreatedPrice = decimal.Round(createdPrice, 8, MidpointRounding.AwayFromZero);
            LastObservedPrice = CreatedPrice;
            Direction = CreatedPrice <= targetPrice ? NotificationAlertDirection.Above : NotificationAlertDirection.Below;
            TargetPrice = decimal.Round(targetPrice, 8, MidpointRounding.AwayFromZero);
            NotifyInApp = notifyInApp;
            NotifyEmail = notifyEmail;
            Notes = LimitOptional(notes, MaxNotesLength);
            IsActive = true;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string Name { get; protected set; }

        public string Symbol { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public NotificationAlertRuleType AlertType { get; protected set; }

        public NotificationAlertDirection Direction { get; protected set; }

        public decimal? CreatedPrice { get; protected set; }

        public decimal? LastObservedPrice { get; protected set; }

        public decimal TargetPrice { get; protected set; }

        public bool NotifyInApp { get; protected set; }

        public bool NotifyEmail { get; protected set; }

        public bool IsActive { get; protected set; }

        public DateTime? LastTriggeredAt { get; protected set; }

        public long? LastNotificationId { get; protected set; }

        public string Notes { get; protected set; }

        public bool ShouldTrigger(decimal currentPrice)
        {
            return ShouldTrigger(currentPrice, null, null);
        }

        public bool ShouldTrigger(decimal currentPrice, decimal? bidPrice, decimal? askPrice)
        {
            var normalizedCurrentPrice = RoundPrice(currentPrice);
            var previousPrice = GetReferencePrice();
            var currentLow = GetCurrentLow(normalizedCurrentPrice, bidPrice, askPrice);
            var currentHigh = GetCurrentHigh(normalizedCurrentPrice, bidPrice, askPrice);

            return HasCrossed(previousPrice, currentLow, currentHigh);
        }

        public bool RefreshObservedPrice(decimal currentPrice)
        {
            var normalizedCurrentPrice = RoundPrice(currentPrice);
            if (normalizedCurrentPrice <= 0m)
            {
                return false;
            }

            if (LastObservedPrice.HasValue && LastObservedPrice.Value == normalizedCurrentPrice)
            {
                return false;
            }

            LastObservedPrice = normalizedCurrentPrice;
            return true;
        }

        public void Trigger(long notificationId, DateTime occurredAt, decimal currentPrice)
        {
            LastNotificationId = notificationId;
            LastTriggeredAt = occurredAt;
            LastObservedPrice = RoundPrice(currentPrice);
            IsActive = false;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        private decimal GetReferencePrice()
        {
            if (LastObservedPrice.HasValue && LastObservedPrice.Value > 0m)
            {
                return LastObservedPrice.Value;
            }

            if (CreatedPrice.HasValue && CreatedPrice.Value > 0m)
            {
                return CreatedPrice.Value;
            }

            return TargetPrice;
        }

        private bool HasCrossed(decimal previousPrice, decimal currentLow, decimal currentHigh)
        {
            if (previousPrice <= 0m || currentLow <= 0m || currentHigh <= 0m)
            {
                return false;
            }

            var movedUpThroughTarget = previousPrice < TargetPrice && currentHigh >= TargetPrice;
            var movedDownThroughTarget = previousPrice > TargetPrice && currentLow <= TargetPrice;

            return movedUpThroughTarget || movedDownThroughTarget;
        }

        private static decimal RoundPrice(decimal price)
        {
            return decimal.Round(price, 8, MidpointRounding.AwayFromZero);
        }

        private static decimal GetCurrentLow(decimal currentPrice, decimal? bidPrice, decimal? askPrice)
        {
            var prices = new[] { currentPrice, RoundNullablePrice(bidPrice), RoundNullablePrice(askPrice) }
                .Where(x => x.HasValue && x.Value > 0m)
                .Select(x => x.Value)
                .ToList();

            return prices.Count == 0 ? currentPrice : prices.Min();
        }

        private static decimal GetCurrentHigh(decimal currentPrice, decimal? bidPrice, decimal? askPrice)
        {
            var prices = new[] { currentPrice, RoundNullablePrice(bidPrice), RoundNullablePrice(askPrice) }
                .Where(x => x.HasValue && x.Value > 0m)
                .Select(x => x.Value)
                .ToList();

            return prices.Count == 0 ? currentPrice : prices.Max();
        }

        private static decimal? RoundNullablePrice(decimal? price)
        {
            return price.HasValue && price.Value > 0m
                ? RoundPrice(price.Value)
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
