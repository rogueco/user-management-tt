using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.Extensions;
using UserManagement.Services.Results;

namespace UserManagement.UnitTests.Extensions;

public class ControllerExtensionsTests
{
    private sealed class TestController : ControllerBase;

    private readonly TestController _controller = new() { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };

    [Fact]
    public void Success_MustMapToOkWithValue()
        => _controller.ServiceResultToActionResult(ServiceResult<string>.Success("value"))
            .Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("value");

    [Fact]
    public void Success_MustUseTheSuppliedResultWhenGiven()
        => _controller.ServiceResultToActionResult(ServiceResult<string>.Success("value"), v => _controller.Accepted(v))
            .Should().BeOfType<AcceptedResult>();

    [Fact]
    public void SuccessWithoutValue_MustMapToNoContent()
        => _controller.ServiceResultToActionResult(ServiceResult.Success()).Should().BeOfType<NoContentResult>();

    [Theory]
    [InlineData(ServiceResultCode.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ServiceResultCode.InvalidInput, StatusCodes.Status400BadRequest)]
    [InlineData(ServiceResultCode.InternalError, StatusCodes.Status500InternalServerError)]
    public void Failures_MustMapToProblemDetailsWithTheirStatus(ServiceResultCode code, int expectedStatus)
    {
        var result = code switch
        {
            ServiceResultCode.NotFound => ServiceResult<string>.NotFound("gone"),
            ServiceResultCode.InvalidInput => ServiceResult<string>.InvalidInput("bad"),
            _ => ServiceResult<string>.InternalError("broken")
        };

        var objectResult = _controller.ServiceResultToActionResult(result).Should().BeOfType<ObjectResult>().Subject;

        objectResult.StatusCode.Should().Be(expectedStatus);
        objectResult.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(result.ErrorMessage);
    }

    [Fact]
    public void InvalidInputWithFieldErrors_MustMapToValidationProblem()
    {
        var result = ServiceResult<string>.InvalidInput("bad", new Dictionary<string, string[]> { ["Email"] = ["required"] });

        var objectResult = _controller.ServiceResultToActionResult(result).Should().BeOfType<BadRequestObjectResult>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>().Which.Errors.Should().ContainKey("Email");
    }
}
