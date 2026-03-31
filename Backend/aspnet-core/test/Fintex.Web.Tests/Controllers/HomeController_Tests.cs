using Fintex.Models.TokenAuth;
using Fintex.Web.Controllers;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Fintex.Web.Tests.Controllers;

public class HomeController_Tests : FintexWebTestBase
{
    [Fact]
    public async Task Index_Test()
    {
        await AuthenticateAsync(null, new AuthenticateModel
        {
            UserNameOrEmailAddress = "admin",
            Password = "123qwe"
        });

        //Act
        var response = await GetResponseAsStringAsync(
            GetUrl<HomeController>(nameof(HomeController.Index))
        );

        //Assert
        response.ShouldNotBeNullOrEmpty();
    }
}