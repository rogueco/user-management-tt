using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.API.Controllers.V1;
using UserManagement.Contracts.Logs;
using UserManagement.Services.Results;
using UserManagement.Services.Services.LogServices;

namespace UserManagement.UnitTests.Controllers.V1.LogsControllerTests;

public class LogsControllerTests
{
    private readonly Mock<IUserLogService> _mockLogService = new();
    private readonly LogsController _controller;

    public LogsControllerTests()
        => _controller = new LogsController(_mockLogService.Object, Mock.Of<ILogger<LogsController>>());

    [Fact]
    public void Ctor_WhenServiceNull_MustThrow()
    {
        var act = () => new LogsController(null!, Mock.Of<ILogger<LogsController>>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("userLogService");
    }

    [Fact]
    public async Task GetById_WhenFound_MustReturnOkWithEntry()
    {
        var details = new LogDetailsDto(new LogDto(1, 1, "Peter Loew", "ploew@example.com", LogAction.Created, DateTime.UtcNow), [], true);
        _mockLogService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<LogDetailsDto>.Success(details));

        var result = await _controller.GetById(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(details);
    }

    [Fact]
    public async Task GetById_WhenMissing_MustReturnNotFound()
    {
        _mockLogService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<LogDetailsDto>.NotFound("missing"));

        var result = await _controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
