using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserManagement.API.Extensions;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Services.Services.LogServices;

namespace UserManagement.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly IUserLogService _userLogService;
    private readonly ILogger<LogsController> _logger;

    public LogsController(IUserLogService userLogService, ILogger<LogsController> logger)
    {
        _userLogService = userLogService ?? throw new ArgumentNullException(nameof(userLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a page of log entries, newest first, optionally filtered by user, action and name or email
    /// </summary>
    /// <response code="200">The page of log entries</response>
    [HttpGet]
    [SwaggerOperation(OperationId = "Logs_GetPage")]
    [ProducesResponseType(typeof(PagedResult<LogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPage([FromQuery] LogFilter filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => this.ServiceResultToActionResult(await _userLogService.GetPageAsync(filter, page, pageSize, cancellationToken));

    /// <summary>
    /// Retrieves a single log entry with the fields it changed
    /// </summary>
    /// <param name="id">The id of the log entry</param>
    /// <response code="200">The log entry</response>
    /// <response code="404">The log entry does not exist</response>
    [HttpGet("{id:long}")]
    [SwaggerOperation(OperationId = "Logs_GetById")]
    [ProducesResponseType(typeof(LogDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => this.ServiceResultToActionResult(await _userLogService.GetByIdAsync(id, cancellationToken));
}
