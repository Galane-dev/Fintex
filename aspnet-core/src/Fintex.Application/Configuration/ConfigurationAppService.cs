using Abp.Authorization;
using Abp.Runtime.Session;
using Fintex.Configuration.Dto;
using System.Threading.Tasks;

namespace Fintex.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : FintexAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
