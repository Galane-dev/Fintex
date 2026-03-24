using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Fintex.Controllers
{
    public abstract class FintexControllerBase : AbpController
    {
        protected FintexControllerBase()
        {
            LocalizationSourceName = FintexConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
