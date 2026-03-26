using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Brokers.Dto
{
    public class ConnectExternalBrokerAccountInput
    {
        [Required]
        [MaxLength(ExternalBrokerConnection.MaxDisplayNameLength)]
        public string DisplayName { get; set; }

        [Required]
        public ExternalBrokerProvider Provider { get; set; }

        [Required]
        public ExternalBrokerPlatform Platform { get; set; }

        [Required]
        [MaxLength(ExternalBrokerConnection.MaxLoginLength)]
        public string AccountLogin { get; set; }

        [Required]
        [MaxLength(ExternalBrokerConnection.MaxServerLength)]
        public string Server { get; set; }

        [Required]
        public string Password { get; set; }

        [MaxLength(ExternalBrokerConnection.MaxTerminalPathLength)]
        public string TerminalPath { get; set; }
    }
}
