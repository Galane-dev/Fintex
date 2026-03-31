using Abp.Application.Services;
using Fintex.MultiTenancy.Dto;

namespace Fintex.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

