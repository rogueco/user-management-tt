using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;
using UserManagement.Services.Results;
using UserManagement.UnitTests.Builders;

namespace UserManagement.UnitTests.Controllers.V1.UsersControllerTests;

public class GetTests : TestBase
{
    [Fact]
    public async Task GetById_WhenServiceFindsUser_MustReturnOkWithUser()
    {
        var user = new UserDtoBuilder().Build();
        MockUserService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<UserDto>.Success(user));

        var result = await Controller.GetById(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(user);
    }

    [Fact]
    public async Task GetById_WhenServiceDoesNotFindUser_MustReturnNotFoundProblem()
    {
        MockUserService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<UserDto>.NotFound("User 99 was not found."));

        var result = await Controller.GetById(99, CancellationToken.None);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        objectResult.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be("User 99 was not found.");
    }

    [Fact]
    public async Task GetPage_MustPassFilterAndPagingToService()
    {
        var filter = new UserFilter { IsActive = false };
        var page = new PagedResult<UserDto>([new UserDtoBuilder().Inactive().Build()], 2, 3, 41);
        MockUserService.Setup(s => s.GetPageAsync(filter, 2, 20, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<PagedResult<UserDto>>.Success(page));

        var result = await Controller.GetPage(filter, 2, 20, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(page);
        MockUserService.Verify(s => s.GetPageAsync(filter, 2, 20, It.IsAny<CancellationToken>()), Times.Once());
    }
}
