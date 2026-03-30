using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Fintex.Investments
{
    /// <summary>
    /// Stores a user-owned external broker account that is connected through a platform bridge.
    /// </summary>
    public class ExternalBrokerConnection : FullAuditedAggregateRoot<long>, IMayHaveTenant
    {
        public const int MaxDisplayNameLength = 64;
        public const int MaxLoginLength = 32;
        public const int MaxServerLength = 128;
        public const int MaxTerminalPathLength = 260;
        public const int MaxErrorLength = 512;
        public const int MaxAccountNameLength = 128;
        public const int MaxCurrencyLength = 16;
        public const int MaxCompanyLength = 128;

        protected ExternalBrokerConnection()
        {
        }

        public ExternalBrokerConnection(
            int? tenantId,
            long userId,
            string displayName,
            ExternalBrokerProvider provider,
            ExternalBrokerPlatform platform,
            string accountLogin,
            string server,
            string encryptedPassword,
            string terminalPath)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User id must be provided.", nameof(userId));
            }

            TenantId = tenantId;
            UserId = userId;
            DisplayName = Limit(displayName, MaxDisplayNameLength, "Display name is required.");
            Provider = provider;
            Platform = platform;
            AccountLogin = Limit(accountLogin, MaxLoginLength, "Account login is required.");
            Server = Limit(server, MaxServerLength, "Server is required.");
            EncryptedPassword = Require(encryptedPassword, "Encrypted password is required.");
            TerminalPath = LimitOptional(terminalPath, MaxTerminalPathLength);
            Status = ExternalBrokerConnectionStatus.Pending;
            IsActive = true;
        }

        public int? TenantId { get; set; }

        public long UserId { get; protected set; }

        public string DisplayName { get; protected set; }

        public ExternalBrokerProvider Provider { get; protected set; }

        public ExternalBrokerPlatform Platform { get; protected set; }

        public string AccountLogin { get; protected set; }

        public string Server { get; protected set; }

        public string EncryptedPassword { get; protected set; }

        public string TerminalPath { get; protected set; }

        public ExternalBrokerConnectionStatus Status { get; protected set; }

        public bool IsActive { get; protected set; }

        public DateTime? LastValidatedAt { get; protected set; }

        public DateTime? LastSyncedAt { get; protected set; }

        public string LastError { get; protected set; }

        public string BrokerAccountName { get; protected set; }

        public string BrokerAccountCurrency { get; protected set; }

        public string BrokerCompany { get; protected set; }

        public int? BrokerLeverage { get; protected set; }

        public decimal? LastKnownBalance { get; protected set; }

        public decimal? LastKnownEquity { get; protected set; }

        public void UpdateCredentials(
            string displayName,
            string server,
            string encryptedPassword,
            string terminalPath)
        {
            DisplayName = Limit(displayName, MaxDisplayNameLength, "Display name is required.");
            Server = Limit(server, MaxServerLength, "Server is required.");
            EncryptedPassword = Require(encryptedPassword, "Encrypted password is required.");
            TerminalPath = LimitOptional(terminalPath, MaxTerminalPathLength);
            LastError = null;
        }

        public void MarkConnected(
            string brokerAccountName,
            string brokerAccountCurrency,
            string brokerCompany,
            int? brokerLeverage,
            decimal? balance,
            decimal? equity,
            DateTime occurredAt)
        {
            BrokerAccountName = LimitOptional(brokerAccountName, MaxAccountNameLength);
            BrokerAccountCurrency = LimitOptional(brokerAccountCurrency, MaxCurrencyLength);
            BrokerCompany = LimitOptional(brokerCompany, MaxCompanyLength);
            BrokerLeverage = brokerLeverage;
            LastKnownBalance = balance;
            LastKnownEquity = equity;
            LastValidatedAt = occurredAt;
            LastSyncedAt = occurredAt;
            LastError = null;
            Status = ExternalBrokerConnectionStatus.Connected;
            IsActive = true;
        }

        public void MarkFailed(string error, DateTime occurredAt)
        {
            LastValidatedAt = occurredAt;
            LastError = LimitOptional(error, MaxErrorLength);
            Status = ExternalBrokerConnectionStatus.Failed;
        }

        public void Disconnect(DateTime occurredAt)
        {
            IsActive = false;
            Status = ExternalBrokerConnectionStatus.Disconnected;
            LastSyncedAt = occurredAt;
        }

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(error, nameof(value));
            }

            return value.Trim();
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

        private static string LimitOptional(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed.Substring(0, maxLength);
        }
    }
}
