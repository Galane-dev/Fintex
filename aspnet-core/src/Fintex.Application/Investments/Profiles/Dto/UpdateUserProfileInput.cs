using System.ComponentModel.DataAnnotations;

namespace Fintex.Investments.Profiles.Dto
{
    /// <summary>
    /// Input for updating the authenticated user's profile.
    /// </summary>
    public class UpdateUserProfileInput
    {
        [MaxLength(UserProfile.MaxCurrencyLength)]
        public string PreferredBaseCurrency { get; set; }

        [MaxLength(UserProfile.MaxSymbolsLength)]
        public string FavoriteSymbols { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal RiskTolerance { get; set; }

        public bool IsAiInsightsEnabled { get; set; }

        [MaxLength(UserProfile.MaxStrategyLength)]
        public string StrategyNotes { get; set; }
    }
}
