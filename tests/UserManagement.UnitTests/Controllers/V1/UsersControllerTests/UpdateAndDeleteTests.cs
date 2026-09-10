using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Contracts.Users;
using UserManagement.Services.Results;
using UserManagement.UnitTests.Builders;

namespace UserManagement.UnitTests.Controllers.V1.UsersControllerTests;

public class UpdateAndDeleteTests : TestBase
{
    [Fact]
    public async Task Update_WhenServiceSucceeds_MustReturnOkWithUser()
    {
        var request = new UserRequestBuilder().WithSurname("Renamed").Build();
        var updated = new UserDtoBuilder().WithName("Testy", "Renamed").Build();
        MockUserService.Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<UserDto>.Success(updated));

        var result = await Controller.Update(1, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_MustReturnNotFound()
    {
        MockUserService.Setup(s => s.UpdateAsync(99, It.IsAny<UserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<UserDto>.NotFound("missing"));

        var result = await Controller.Update(99, new UserRequestBuilder().Build(), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Delete_WhenServiceSucceeds_MustReturnNoContent()
    {
        MockUserService.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult.Success());

        var result = await Controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_MustReturnNotFound()
    {
        MockUserService.Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult.NotFound("missing"));

        var result = await Controller.Delete(99, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
