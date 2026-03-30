using Abp.Application.Services;
using Fintex.Investments.Profiles.Dto;
using System.Threading.Tasks;

namespace Fintex.Investments.Profiles
{
    /// <summary>
    /// Application contract for user investment profiles.
    /// </summary>
    public interface IUserProfileAppService : IApplicationService
    {
        Task<UserProfileDto> GetMyProfileAsync();

        Task<UserProfileDto> UpdateMyProfileAsync(UpdateUserProfileInput input);
    }
}
