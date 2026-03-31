using Abp.Application.Services;
using Fintex.Sessions.Dto;
using System.Threading.Tasks;

namespace Fintex.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
