using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Fintex.EntityFrameworkCore;
using Fintex.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Fintex.Web.Tests;

[DependsOn(
    typeof(FintexWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class FintexWebTestModule : AbpModule
{
    public FintexWebTestModule(FintexEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(FintexWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(FintexWebMvcModule).Assembly);
    }
}