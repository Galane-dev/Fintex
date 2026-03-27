using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Aggregate root that tracks a user trade and its live performance.
    /// </summary>
    public class Trade : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSymbolLength = 32;
        public const int MaxNotesLength = 2048;
        public const int MaxExternalOrderIdLength = 128;

        protected Trade()
        {
        }

        public Trade(
            int? tenantId,
            long userId,
            string symbol,
            AssetClass assetClass,
            MarketDataProvider provider,
            TradeDirection direction,
            decimal quantity,
            decimal entryPrice,
            DateTime executedAt,
            decimal? stopLoss,
            decimal? takeProfit,
            string externalOrderId,
            string notes)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User id must be provided.", nameof(userId));
            }

            TenantId = tenantId;
            UserId = userId;
            Symbol = NormalizeSymbol(symbol);
            AssetClass = assetClass;
            Provider = provider;
            Direction = direction;
            Quantity = EnsurePositive(quantity, nameof(quantity));
            EntryPrice = EnsurePositive(entryPrice, nameof(entryPrice));
            ExecutedAt = executedAt;
            StopLoss = stopLoss;
            TakeProfit = takeProfit;
            ExternalOrderId = Limit(externalOrderId, MaxExternalOrderIdLength);
            Notes = Limit(notes, MaxNotesLength);
            Status = TradeStatus.Open;
            LastMarketPrice = entryPrice;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string Symbol { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public TradeDirection Direction { get; protected set; }

        public TradeStatus Status { get; protected set; }

        public decimal Quantity { get; protected set; }

        public decimal EntryPrice { get; protected set; }

        public decimal? ExitPrice { get; protected set; }

        public decimal? StopLoss { get; protected set; }

        public decimal? TakeProfit { get; protected set; }

        public decimal RealizedProfitLoss { get; protected set; }

        public decimal UnrealizedProfitLoss { get; protected set; }

        public decimal LastMarketPrice { get; protected set; }

        public decimal CurrentRiskScore { get; protected set; }

        public string CurrentRecommendation { get; protected set; }

        public string CurrentAnalysisSummary { get; protected set; }

        public string ExternalOrderId { get; protected set; }

        public string Notes { get; protected set; }

        public DateTime ExecutedAt { get; protected set; }

        public DateTime? ClosedAt { get; protected set; }

        /// <summary>
        /// Updates the trade with the latest market price and recalculates unrealized P/L.
        /// </summary>
        public void RefreshMarketPrice(decimal marketPrice)
        {
            LastMarketPrice = EnsurePositive(marketPrice, nameof(marketPrice));
            UnrealizedProfitLoss = CalculateProfitLoss(EntryPrice, marketPrice, Quantity, Direction);
        }

        /// <summary>
        /// Closes the trade at the supplied price and timestamps the execution.
        /// </summary>
        public void Close(decimal exitPrice, DateTime closedAt)
        {
            if (Status != TradeStatus.Open)
            {
                throw new InvalidOperationException("Only open trades can be closed.");
            }

            ExitPrice = EnsurePositive(exitPrice, nameof(exitPrice));
            ClosedAt = closedAt;
            Status = TradeStatus.Closed;
            LastMarketPrice = exitPrice;
            RealizedProfitLoss = CalculateProfitLoss(EntryPrice, exitPrice, Quantity, Direction);
            UnrealizedProfitLoss = 0m;
        }

        /// <summary>
        /// Cancels the trade before settlement.
        /// </summary>
        public void Cancel(string notes)
        {
            if (Status == TradeStatus.Closed)
            {
                throw new InvalidOperationException("Closed trades cannot be cancelled.");
            }

            Status = TradeStatus.Cancelled;
            Notes = string.IsNullOrWhiteSpace(notes)
                ? Notes
                : Limit(notes, MaxNotesLength);
        }

        /// <summary>
        /// Applies the latest analytics summary to the trade aggregate.
        /// </summary>
        public void ApplyAnalysis(decimal riskScore, TradeRecommendation recommendation, string summary)
        {
            CurrentRiskScore = Clamp(riskScore, 0m, 100m);
            CurrentRecommendation = recommendation.ToString();
            CurrentAnalysisSummary = Limit(summary, MaxNotesLength);
        }

        public static decimal CalculateProfitLoss(decimal entryPrice, decimal exitPrice, decimal quantity, TradeDirection direction)
        {
            var delta = direction == TradeDirection.Buy
                ? exitPrice - entryPrice
                : entryPrice - exitPrice;

            return decimal.Round(delta * quantity, 8, MidpointRounding.AwayFromZero);
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

            return value.Length <= maxLength
                ? value.Trim()
                : value.Trim().Substring(0, maxLength);
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
