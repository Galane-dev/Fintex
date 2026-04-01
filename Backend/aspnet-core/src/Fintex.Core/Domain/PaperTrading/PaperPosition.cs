using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Netted simulated position for an account and market instrument.
    /// </summary>
    public class PaperPosition : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxSymbolLength = 32;

        protected PaperPosition()
        {
        }

        public PaperPosition(
            int? tenantId,
            long userId,
            long accountId,
            string symbol,
            AssetClass assetClass,
            MarketDataProvider provider,
            TradeDirection direction,
            decimal quantity,
            decimal entryPrice,
            decimal? stopLoss,
            decimal? takeProfit,
            DateTime openedAt)
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
            Quantity = EnsurePositive(quantity, nameof(quantity));
            AverageEntryPrice = EnsurePositive(entryPrice, nameof(entryPrice));
            CurrentMarketPrice = AverageEntryPrice;
            StopLoss = stopLoss;
            TakeProfit = takeProfit;
            OpenedAt = openedAt;
            LastUpdatedAt = openedAt;
            Status = PaperPositionStatus.Open;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public long AccountId { get; protected set; }

        public string Symbol { get; protected set; }

        public AssetClass AssetClass { get; protected set; }

        public MarketDataProvider Provider { get; protected set; }

        public TradeDirection Direction { get; protected set; }

        public PaperPositionStatus Status { get; protected set; }

        public decimal Quantity { get; protected set; }

        public decimal AverageEntryPrice { get; protected set; }

        public decimal CurrentMarketPrice { get; protected set; }

        public decimal RealizedProfitLoss { get; protected set; }

        public decimal UnrealizedProfitLoss { get; protected set; }

        public decimal? StopLoss { get; protected set; }

        public decimal? TakeProfit { get; protected set; }

        public DateTime OpenedAt { get; protected set; }

        public DateTime LastUpdatedAt { get; protected set; }

        public DateTime? ClosedAt { get; protected set; }

        public void Add(decimal quantity, decimal fillPrice, DateTime occurredAt)
        {
            EnsureOpen();
            var nextQuantity = Quantity + EnsurePositive(quantity, nameof(quantity));
            AverageEntryPrice = decimal.Round(
                ((AverageEntryPrice * Quantity) + (fillPrice * quantity)) / nextQuantity,
                8,
                MidpointRounding.AwayFromZero);
            Quantity = nextQuantity;
            RefreshMarketPrice(fillPrice, occurredAt);
        }

        public decimal Reduce(decimal quantity, decimal fillPrice, DateTime occurredAt)
        {
            EnsureOpen();
            var reductionQuantity = EnsurePositive(quantity, nameof(quantity));
            if (reductionQuantity > Quantity)
            {
                throw new InvalidOperationException("Cannot reduce a position by more than its current quantity.");
            }

            var realized = Trade.CalculateProfitLoss(AverageEntryPrice, fillPrice, reductionQuantity, Direction);
            RealizedProfitLoss += realized;
            Quantity -= reductionQuantity;
            CurrentMarketPrice = fillPrice;
            LastUpdatedAt = occurredAt;

            if (Quantity == 0m)
            {
                UnrealizedProfitLoss = 0m;
                Status = PaperPositionStatus.Closed;
                ClosedAt = occurredAt;
            }
            else
            {
                UnrealizedProfitLoss = Trade.CalculateProfitLoss(
                    AverageEntryPrice,
                    CurrentMarketPrice,
                    Quantity,
                    Direction);
            }

            return realized;
        }

        public void RefreshMarketPrice(decimal marketPrice, DateTime occurredAt)
        {
            CurrentMarketPrice = EnsurePositive(marketPrice, nameof(marketPrice));
            UnrealizedProfitLoss = Status == PaperPositionStatus.Open
                ? Trade.CalculateProfitLoss(AverageEntryPrice, CurrentMarketPrice, Quantity, Direction)
                : 0m;
            LastUpdatedAt = occurredAt;
        }

        public void ApplyTradePlan(decimal? stopLoss, decimal? takeProfit, DateTime occurredAt)
        {
            EnsureOpen();
            StopLoss = stopLoss;
            TakeProfit = takeProfit;
            LastUpdatedAt = occurredAt;
        }

        public PaperPositionRiskTrigger GetTriggeredRiskExit(decimal marketPrice)
        {
            EnsurePositive(marketPrice, nameof(marketPrice));

            if (Status != PaperPositionStatus.Open)
            {
                return PaperPositionRiskTrigger.None;
            }

            if (Direction == TradeDirection.Buy)
            {
                if (StopLoss.HasValue && marketPrice <= StopLoss.Value)
                {
                    return PaperPositionRiskTrigger.StopLoss;
                }

                if (TakeProfit.HasValue && marketPrice >= TakeProfit.Value)
                {
                    return PaperPositionRiskTrigger.TakeProfit;
                }

                return PaperPositionRiskTrigger.None;
            }

            if (StopLoss.HasValue && marketPrice >= StopLoss.Value)
            {
                return PaperPositionRiskTrigger.StopLoss;
            }

            if (TakeProfit.HasValue && marketPrice <= TakeProfit.Value)
            {
                return PaperPositionRiskTrigger.TakeProfit;
            }

            return PaperPositionRiskTrigger.None;
        }

        private void EnsureOpen()
        {
            if (Status != PaperPositionStatus.Open)
            {
                throw new InvalidOperationException("Only open positions can be updated.");
            }
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
    }
}
