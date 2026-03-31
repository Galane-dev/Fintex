using Abp.Modules;
using Abp.Reflection.Extensions;
using Fintex.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Fintex.Web.Host.Startup
{
    [DependsOn(
       typeof(FintexWebCoreModule))]
    public class FintexWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public FintexWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(FintexWebHostModule).GetAssembly());
        }
    }
}
