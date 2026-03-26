using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// User-submitted simulated order that is filled by the paper execution engine.
    /// </summary>
    public class PaperOrder : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSymbolLength = 32;
        public const int MaxNotesLength = 2048;

        protected PaperOrder()
        {
        }

        public PaperOrder(
            int? tenantId,
            long userId,
            long accountId,
            string symbol,
            AssetClass assetClass,
            MarketDataProvider provider,
            TradeDirection direction,
            PaperOrderType orderType,
            decimal quantity,
            decimal? requestedPrice,
            decimal? stopLoss,
            decimal? takeProfit,
            string notes,
            DateTime submittedAt)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User id must be provided.", nameof(userId));
            }

            if (accountId <= 0)
            {
                throw new ArgumentException("Account id must be provided.", nameof(accountId));
            }

            TenantId = tenantId;
            UserId = userId;
            AccountId = accountId;
            Symbol = NormalizeSymbol(symbol);
            AssetClass = assetClass;
            Provider = provider;
            Direction = direction;
            OrderType = orderType;
            Quantity = EnsurePositive(quantity, nameof(quantity));
            RequestedPrice = requestedPrice;
            StopLoss = stopLoss;
            TakeProfit = takeProfit;
            Notes = Limit(notes, MaxNotesLength);
            SubmittedAt = submittedAt;
            Status = PaperOrderStatus.Pending;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public long AccountId { get; protected set; }

        public long? PositionId { get; protected set; }

        public string Symbol { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public TradeDirection Direction { get; protected set; }

        public PaperOrderType OrderType { get; protected set; }

        public PaperOrderStatus Status { get; protected set; }

        public decimal Quantity { get; protected set; }

        public decimal? RequestedPrice { get; protected set; }

        public decimal? ExecutedPrice { get; protected set; }

        public decimal? StopLoss { get; protected set; }

        public decimal? TakeProfit { get; protected set; }

        public string Notes { get; protected set; }

        public DateTime SubmittedAt { get; protected set; }

        public DateTime? ExecutedAt { get; protected set; }

        public void MarkFilled(decimal executedPrice, DateTime executedAt, long? positionId)
        {
            if (Status != PaperOrderStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be filled.");
            }

            ExecutedPrice = EnsurePositive(executedPrice, nameof(executedPrice));
            ExecutedAt = executedAt;
            PositionId = positionId;
            Status = PaperOrderStatus.Filled;
        }

        public void Cancel(string notes)
        {
            if (Status == PaperOrderStatus.Filled)
            {
                throw new InvalidOperationException("Filled orders cannot be cancelled.");
            }

            Status = PaperOrderStatus.Cancelled;
            Notes = string.IsNullOrWhiteSpace(notes)
                ? Notes
                : Limit(notes, MaxNotesLength);
        }

        public void Reject(string notes)
        {
            if (Status == PaperOrderStatus.Filled)
            {
                throw new InvalidOperationException("Filled orders cannot be rejected.");
            }

            Status = PaperOrderStatus.Rejected;
            Notes = string.IsNullOrWhiteSpace(notes)
                ? Notes
                : Limit(notes, MaxNotesLength);
        }

        private static string NormalizeSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Symbol is required.", nameof(symbol));
            }

            var normalized = symbol.Trim().ToUpperInvariant();
            if (normalized.Length > MaxSymbolLength)
            {
                throw new ArgumentException("Symbol is too long.", nameof(symbol));
            }

            return normalized;
        }

        private static decimal EnsurePositive(decimal value, string name)
        {
            if (value <= 0m)
            {
                throw new ArgumentOutOfRangeException(name, "Value must be greater than zero.");
            }

            return value;
        }

        private static string Limit(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return value.Trim().Length <= maxLength
                ? value.Trim()
                : value.Trim().Substring(0, maxLength);
        }
    }
}
