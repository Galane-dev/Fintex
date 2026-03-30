using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Notifications.Dto
{
    /// <summary>
    /// Request for creating a BTC price alert notification.
    /// </summary>
    public class CreatePriceAlertInput
    {
        [Required]
        [StringLength(NotificationAlertRule.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(NotificationAlertRule.MaxSymbolLength)]
        public string Symbol { get; set; }

        public MarketDataProvider Provider { get; set; } = MarketDataProvider.Binance;

        [Range(typeof(decimal), "0.00000001", "999999999999")]
        public decimal TargetPrice { get; set; }

        public bool NotifyInApp { get; set; } = true;

        public bool NotifyEmail { get; set; } = true;

        [StringLength(NotificationAlertRule.MaxNotesLength)]
        public string Notes { get; set; }
    }
}
