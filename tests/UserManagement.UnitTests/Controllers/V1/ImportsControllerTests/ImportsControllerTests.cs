using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.API.Controllers.V1;
using UserManagement.Contracts.Imports;
using UserManagement.Services.Results;
using UserManagement.Services.Services.ImportServices;

namespace UserManagement.UnitTests.Controllers.V1.ImportsControllerTests;

public class ImportsControllerTests
{
    private readonly Mock<IImportService> _mockImportService = new();
    private readonly ImportsController _controller;

    public ImportsControllerTests()
        => _controller = new ImportsController(_mockImportService.Object, Mock.Of<ILogger<ImportsController>>());

    [Fact]
    public async Task Submit_MustPassFileContentToServiceAndReturnAcceptedAtTheJob()
    {
        var job = new ImportJobDto(Guid.NewGuid(), "users.csv", ImportJobStatus.Pending, 0, 0, 0, [], DateTime.UtcNow, null, null);
        _mockImportService.Setup(s => s.SubmitAsync("users.csv", "a,b,c", It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<ImportJobDto>.Success(job));

        var result = await _controller.Submit(File("users.csv", "a,b,c"), CancellationToken.None);

        var accepted = result.Should().BeOfType<AcceptedAtActionResult>().Subject;
        accepted.ActionName.Should().Be("GetById");
        accepted.RouteValues!["id"].Should().Be(job.Id);
        accepted.Value.Should().Be(job);
    }

    [Fact]
    public async Task Submit_WhenServiceRejectsFile_MustReturnValidationProblem()
    {
        _mockImportService.Setup(s => s.SubmitAsync("empty.csv", "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<ImportJobDto>.InvalidInput("The file is empty.", new Dictionary<string, string[]> { ["file"] = ["The file is empty."] }));

        var result = await _controller.Submit(File("empty.csv", ""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task GetById_WhenMissing_MustReturnNotFound()
    {
        var id = Guid.NewGuid();
        _mockImportService.Setup(s => s.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(ServiceResult<ImportJobDto>.NotFound("missing"));

        var result = await _controller.GetById(id, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    private static IFormFile File(string name, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);
    }
}
