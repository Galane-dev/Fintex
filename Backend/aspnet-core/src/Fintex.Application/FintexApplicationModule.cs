using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Fintex.Authorization;

namespace Fintex;

[DependsOn(
    typeof(FintexCoreModule),
    typeof(AbpAutoMapperModule))]
public class FintexApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<FintexAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(FintexApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
