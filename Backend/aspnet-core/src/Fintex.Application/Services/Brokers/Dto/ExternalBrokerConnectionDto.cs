using System;

namespace Fintex.Investments.Brokers.Dto
{
    public class ExternalBrokerConnectionDto
    {
        public long Id { get; set; }

        public string DisplayName { get; set; }

        public ExternalBrokerProvider Provider { get; set; }

        public ExternalBrokerPlatform Platform { get; set; }

        public string AccountLogin { get; set; }

        public string Server { get; set; }

        public string TerminalPath { get; set; }

        public ExternalBrokerConnectionStatus Status { get; set; }

        public bool IsActive { get; set; }

        public string BrokerAccountName { get; set; }

        public string BrokerAccountCurrency { get; set; }

        public string BrokerCompany { get; set; }

        public int? BrokerLeverage { get; set; }

        public decimal? LastKnownBalance { get; set; }

        public decimal? LastKnownEquity { get; set; }

        public string LastError { get; set; }

        public DateTime? LastValidatedAt { get; set; }

        public DateTime? LastSyncedAt { get; set; }
    }
}
