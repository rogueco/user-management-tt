using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserManagement.API.Extensions;
using UserManagement.Contracts.Imports;
using UserManagement.Services.Services.ImportServices;

namespace UserManagement.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
public class ImportsController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly ILogger<ImportsController> _logger;

    public ImportsController(IImportService importService, ILogger<ImportsController> logger)
    {
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Accepts a CSV of users to import. The import runs in the background; poll the job for progress
    /// </summary>
    /// <param name="file">A CSV with the columns Forename, Surname, Email, DateOfBirth, IsActive</param>
    /// <response code="202">The import job was queued</response>
    /// <response code="400">The file is empty</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(OperationId = "Imports_Submit")]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(IFormFile file, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync(cancellationToken);

        var result = await _importService.SubmitAsync(file.FileName, content, cancellationToken);
        if (result.IsSuccessful)
        {
            _logger.LogInformation("Import {JobId} accepted for {FileName}", result.Value!.Id, file.FileName);
        }

        return this.ServiceResultToActionResult(result, job => AcceptedAtAction(nameof(GetById), new { id = job.Id, version = "1" }, job));
    }

    /// <summary>
    /// Retrieves the most recent import jobs
    /// </summary>
    /// <response code="200">The recent import jobs</response>
    [HttpGet]
    [SwaggerOperation(OperationId = "Imports_GetRecent")]
    [ProducesResponseType(typeof(IReadOnlyList<ImportJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
        => this.ServiceResultToActionResult(await _importService.GetRecentAsync(cancellationToken));

    /// <summary>
    /// Retrieves an import job with its progress and any row errors
    /// </summary>
    /// <param name="id">The id of the import job</param>
    /// <response code="200">The import job</response>
    /// <response code="404">The import job does not exist</response>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(OperationId = "Imports_GetById")]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ServiceResultToActionResult(await _importService.GetAsync(id, cancellationToken));
}
