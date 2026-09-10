using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Contracts.Users;
using UserManagement.Services.Results;
using UserManagement.UnitTests.Builders;
using UserManagement.UnitTests.MockExtensions;

namespace UserManagement.UnitTests.Controllers.V1.UsersControllerTests;

public class CreateTests : TestBase
{
    [Fact]
    public async Task Create_WhenServiceSucceeds_MustReturnCreatedAtTheUser()
    {
        var request = new UserRequestBuilder().Build();
        var created = new UserDtoBuilder().WithId(12).Build();
        MockUserService.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<UserDto>.Success(created));

        var result = await Controller.Create(request, CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetById");
        createdResult.RouteValues!["id"].Should().Be(12L);
        createdResult.Value.Should().Be(created);
        MockLogger.VerifyLogLevelWasCalled(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task Create_WhenServiceRejectsInput_MustReturnValidationProblem()
    {
        var request = new UserRequestBuilder().WithEmail("nope").Build();
        var errors = new Dictionary<string, string[]> { ["Email"] = ["The Email field is not a valid e-mail address."] };
        MockUserService.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<UserDto>.InvalidInput("Invalid", errors));

        var result = await Controller.Create(request, CancellationToken.None);

        var objectResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>().Which.Errors.Should().ContainKey("Email");
        MockLogger.VerifyLogLevelWasCalled(LogLevel.Information, Times.Never());
    }
}
