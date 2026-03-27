using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Brokers.Dto
{
    public class ConnectAlpacaBrokerAccountInput
    {
        [Required]
        [MaxLength(ExternalBrokerConnection.MaxDisplayNameLength)]
        public string DisplayName { get; set; }

        [Required]
        [MaxLength(ExternalBrokerConnection.MaxLoginLength)]
        public string ApiKey { get; set; }

        [Required]
        public string ApiSecret { get; set; }

        public bool IsPaperEnvironment { get; set; } = true;
    }
}
