using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// User-owned simulated trading account used before live broker connectivity.
    /// </summary>
    public class PaperTradingAccount : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxNameLength = 64;
        public const int MaxCurrencyLength = 8;

        protected PaperTradingAccount()
        {
        }

        public PaperTradingAccount(
            int? tenantId,
            long userId,
            string name,
            string baseCurrency,
            decimal startingBalance)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User id must be provided.", nameof(userId));
            }

            TenantId = tenantId;
            UserId = userId;
            Name = Limit(name, MaxNameLength, "Account name is required.");
            BaseCurrency = Limit(baseCurrency, MaxCurrencyLength, "Base currency is required.").ToUpperInvariant();
            StartingBalance = EnsurePositive(startingBalance, nameof(startingBalance));
            CashBalance = StartingBalance;
            Equity = StartingBalance;
            IsActive = true;
            LastMarkedToMarketAt = DateTime.UtcNow;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string Name { get; protected set; }

        public string BaseCurrency { get; protected set; }

        public decimal StartingBalance { get; protected set; }

        public decimal CashBalance { get; protected set; }

        public decimal Equity { get; protected set; }

        public decimal RealizedProfitLoss { get; protected set; }

        public decimal UnrealizedProfitLoss { get; protected set; }

        public bool IsActive { get; protected set; }

        public DateTime LastMarkedToMarketAt { get; protected set; }

        public void ApplyRealizedProfitLoss(decimal realizedProfitLoss, DateTime occurredAt)
        {
            RealizedProfitLoss += realizedProfitLoss;
            CashBalance += realizedProfitLoss;
            Equity = CashBalance + UnrealizedProfitLoss;
            LastMarkedToMarketAt = occurredAt;
        }

        public void ApplyMarkToMarket(decimal unrealizedProfitLoss, DateTime occurredAt)
        {
            UnrealizedProfitLoss = unrealizedProfitLoss;
            Equity = CashBalance + UnrealizedProfitLoss;
            LastMarkedToMarketAt = occurredAt;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        private static decimal EnsurePositive(decimal value, string name)
        {
            if (value <= 0m)
            {
                throw new ArgumentOutOfRangeException(name, "Value must be greater than zero.");
            }

            return value;
        }

        private static string Limit(string value, int maxLength, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(error, nameof(value));
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
