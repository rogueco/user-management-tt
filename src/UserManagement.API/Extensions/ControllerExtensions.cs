using Microsoft.AspNetCore.Mvc;
using UserManagement.Services.Results;

namespace UserManagement.API.Extensions;

public static class ControllerExtensions
{
    // One place that turns a service outcome into a status code. Pass onSuccess for anything other than 200.
    public static IActionResult ServiceResultToActionResult<T>(this ControllerBase controller, ServiceResult<T> result, Func<T, IActionResult>? onSuccess = null)
        => result.ResultCode switch
        {
            ServiceResultCode.Success => onSuccess?.Invoke(result.Value!) ?? controller.Ok(result.Value),
            _ => controller.ToFailure(result)
        };

    public static IActionResult ServiceResultToActionResult(this ControllerBase controller, ServiceResult result)
        => result.ResultCode switch
        {
            ServiceResultCode.Success => controller.NoContent(),
            _ => controller.ToFailure(result)
        };

    private static IActionResult ToFailure(this ControllerBase controller, ServiceResult result) => result.ResultCode switch
    {
        ServiceResultCode.NotFound => controller.Problem(result.ErrorMessage, statusCode: StatusCodes.Status404NotFound),
        ServiceResultCode.InvalidInput when result.Errors.Count > 0 => controller.ValidationProblem(new ValidationProblemDetails(result.Errors.ToDictionary(e => e.Key, e => e.Value)) { Detail = result.ErrorMessage }),
        ServiceResultCode.InvalidInput => controller.Problem(result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest),
        _ => controller.Problem(result.ErrorMessage, statusCode: StatusCodes.Status500InternalServerError)
    };
}
