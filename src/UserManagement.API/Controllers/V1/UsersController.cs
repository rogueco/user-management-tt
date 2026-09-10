using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserManagement.API.Extensions;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;
using UserManagement.Services.Services.UserServices;

namespace UserManagement.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a page of users, optionally filtered by active state
    /// </summary>
    /// <response code="200">The page of users</response>
    [HttpGet]
    [SwaggerOperation(OperationId = "Users_GetPage")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPage([FromQuery] UserFilter filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => this.ServiceResultToActionResult(await _userService.GetPageAsync(filter, page, pageSize, cancellationToken));

    /// <summary>
    /// Retrieves a single user by id
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <response code="200">The user</response>
    /// <response code="404">The user does not exist</response>
    [HttpGet("{id:long}")]
    [SwaggerOperation(OperationId = "Users_GetById")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => this.ServiceResultToActionResult(await _userService.GetByIdAsync(id, cancellationToken));

    /// <summary>
    /// Creates a user
    /// </summary>
    /// <response code="201">The user was created</response>
    /// <response code="400">The request is invalid</response>
    [HttpPost]
    [SwaggerOperation(OperationId = "Users_Create")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);
        if (result.IsSuccessful)
        {
            _logger.LogInformation("User {UserId} created", result.Value!.Id);
        }

        return this.ServiceResultToActionResult(result, user => CreatedAtAction(nameof(GetById), new { id = user.Id, version = "1" }, user));
    }

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <response code="200">The updated user</response>
    /// <response code="400">The request is invalid</response>
    /// <response code="404">The user does not exist</response>
    [HttpPut("{id:long}")]
    [SwaggerOperation(OperationId = "Users_Update")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UserRequest request, CancellationToken cancellationToken)
        => this.ServiceResultToActionResult(await _userService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <response code="204">The user was deleted</response>
    /// <response code="404">The user does not exist</response>
    [HttpDelete("{id:long}")]
    [SwaggerOperation(OperationId = "Users_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => this.ServiceResultToActionResult(await _userService.DeleteAsync(id, cancellationToken));
}
