using Fintex.Configuration.Dto;
using System.Threading.Tasks;

namespace Fintex.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
