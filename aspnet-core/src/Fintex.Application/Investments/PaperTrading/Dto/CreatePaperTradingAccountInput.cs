using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.PaperTrading.Dto
{
    /// <summary>
    /// Creates a new paper trading account for the current user.
    /// </summary>
    public class CreatePaperTradingAccountInput
    {
        [Required]
        [MaxLength(PaperTradingAccount.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [MaxLength(PaperTradingAccount.MaxCurrencyLength)]
        public string BaseCurrency { get; set; } = "USD";

        [Range(1, 1000000000)]
        public decimal StartingBalance { get; set; } = 10000m;
    }
}
