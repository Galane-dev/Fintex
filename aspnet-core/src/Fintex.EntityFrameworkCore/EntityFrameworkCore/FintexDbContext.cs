using Abp.Zero.EntityFrameworkCore;
using Fintex.Authorization.Roles;
using Fintex.Authorization.Users;
using Fintex.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Fintex.EntityFrameworkCore;

public class FintexDbContext : AbpZeroDbContext<Tenant, Role, User, FintexDbContext>
{
    /* Define a DbSet for each entity of the application */

    public FintexDbContext(DbContextOptions<FintexDbContext> options)
        : base(options)
    {
    }
}
