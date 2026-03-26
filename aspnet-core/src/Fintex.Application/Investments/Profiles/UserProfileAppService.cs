using Abp.Authorization;
using Abp.Runtime.Session;
using Fintex.Investments.Profiles.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Profiles
{
    /// <summary>
    /// Manages preferences used by trade analytics and AI insights.
    /// </summary>
    [AbpAuthorize]
    public class UserProfileAppService : FintexAppServiceBase, IUserProfileAppService
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public UserProfileAppService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task<UserProfileDto> GetMyProfileAsync()
        {
            var profile = await GetOrCreateAsync();
            return ObjectMapper.Map<UserProfileDto>(profile);
        }

        public async Task<UserProfileDto> UpdateMyProfileAsync(UpdateUserProfileInput input)
        {
            var profile = await GetOrCreateAsync();
            profile.UpdatePreferences(
                input.PreferredBaseCurrency,
                input.FavoriteSymbols,
                input.RiskTolerance,
                input.IsAiInsightsEnabled,
                input.StrategyNotes);

            await _userProfileRepository.UpdateAsync(profile);
            return ObjectMapper.Map<UserProfileDto>(profile);
        }

        private async Task<UserProfile> GetOrCreateAsync()
        {
            var userId = AbpSession.GetUserId();
            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile != null)
            {
                return profile;
            }

            profile = new UserProfile(AbpSession.TenantId, userId, "USD");
            await _userProfileRepository.InsertAsync(profile);
            return profile;
        }
    }
}
