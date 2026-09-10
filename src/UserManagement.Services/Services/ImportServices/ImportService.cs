using Microsoft.Extensions.Logging;
using UserManagement.Contracts.Imports;
using UserManagement.Domain.Imports;
using UserManagement.Services.Messaging;
using UserManagement.Services.Messaging.Messages;
using UserManagement.Services.Repositories;
using UserManagement.Services.Results;

namespace UserManagement.Services.Services.ImportServices;

public class ImportService : IImportService
{
    private const int RecentCount = 20;

    private readonly IImportJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publisher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ImportService> _logger;

    public ImportService(IImportJobRepository jobs, IUnitOfWork unitOfWork, IEventPublisher publisher, TimeProvider timeProvider, ILogger<ImportService> logger)
    {
        _jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // The job row and the message that triggers the worker commit together through the outbox.
    public async Task<ServiceResult<ImportJobDto>> SubmitAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ServiceResult<ImportJobDto>.InvalidInput("The file is empty.", new Dictionary<string, string[]> { ["file"] = ["The file is empty."] });
        }

        var job = ImportJob.Create(fileName, content, _timeProvider.GetUtcNow().UtcDateTime);
        _jobs.Add(job);
        await _publisher.PublishAsync(new ImportRequested(job.Id), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Import job {JobId} queued for {FileName}", job.Id, fileName);
        return ServiceResult<ImportJobDto>.Success(ToDto(job));
    }

    public async Task<ServiceResult<ImportJobDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await _jobs.GetByIdAsync(id, cancellationToken) is { } job
            ? ServiceResult<ImportJobDto>.Success(ToDto(job))
            : ServiceResult<ImportJobDto>.NotFound($"Import job {id} was not found.");

    public async Task<ServiceResult<IReadOnlyList<ImportJobDto>>> GetRecentAsync(CancellationToken cancellationToken = default)
        => ServiceResult<IReadOnlyList<ImportJobDto>>.Success((await _jobs.GetRecentAsync(RecentCount, cancellationToken)).Select(ToDto).ToList());

    public static ImportJobDto ToDto(ImportJob job) => new(
        job.Id,
        job.FileName,
        (ImportJobStatus)job.Status,
        job.TotalRows,
        job.ProcessedRows,
        job.FailedRows,
        job.Errors.Select(e => new ImportErrorDto(e.Row, e.Message)).ToList(),
        job.CreatedAt,
        job.StartedAt,
        job.CompletedAt);
}
