using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Fintex.Configuration;
using Fintex.EntityFrameworkCore;
using Fintex.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace Fintex.Migrator;

[DependsOn(typeof(FintexEntityFrameworkModule))]
public class FintexMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public FintexMigratorModule(FintexEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(FintexMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            FintexConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(FintexMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
