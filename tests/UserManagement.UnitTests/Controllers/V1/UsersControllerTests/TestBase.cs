using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.API.Controllers.V1;
using UserManagement.Services.Services.UserServices;

namespace UserManagement.UnitTests.Controllers.V1.UsersControllerTests;

public abstract class TestBase
{
    protected Mock<IUserService> MockUserService { get; } = new();
    protected Mock<ILogger<UsersController>> MockLogger { get; } = new();
    protected UsersController Controller { get; }

    protected TestBase()
        => Controller = new UsersController(MockUserService.Object, MockLogger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
}
