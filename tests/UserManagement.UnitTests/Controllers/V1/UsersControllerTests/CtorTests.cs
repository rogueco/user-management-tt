using Microsoft.Extensions.Logging;
using UserManagement.API.Controllers.V1;
using UserManagement.Services.Services.UserServices;

namespace UserManagement.UnitTests.Controllers.V1.UsersControllerTests;

public class CtorTests
{
    [Fact]
    public void Ctor_WhenUserServiceNull_MustThrow()
    {
        var act = () => new UsersController(null!, Mock.Of<ILogger<UsersController>>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("userService");
    }

    [Fact]
    public void Ctor_WhenLoggerNull_MustThrow()
    {
        var act = () => new UsersController(Mock.Of<IUserService>(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}
