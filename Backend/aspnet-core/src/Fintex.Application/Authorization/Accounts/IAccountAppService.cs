using Abp.Application.Services;
using Fintex.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace Fintex.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
