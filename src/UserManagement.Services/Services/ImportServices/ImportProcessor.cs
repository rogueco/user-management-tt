using Microsoft.Extensions.Logging;
using UserManagement.Domain.Imports;
using UserManagement.Services.Repositories;
using UserManagement.Services.Services.UserServices;

namespace UserManagement.Services.Services.ImportServices;

public class ImportProcessor
{
    private readonly IImportJobRepository _jobs;
    private readonly IUserService _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ImportProcessor> _logger;

    public ImportProcessor(IImportJobRepository jobs, IUserService users, IUnitOfWork unitOfWork, TimeProvider timeProvider, ILogger<ImportProcessor> logger)
    {
        _jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _jobs.GetByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status != ImportStatus.Pending)
        {
            return;
        }

        try
        {
            var rows = CsvUsers.Parse(job.Content);
            job.Start(rows.Count, Now());

            foreach (var row in rows)
            {
                await ImportRowAsync(job, row, cancellationToken);
            }

            job.Complete(Now());
            _logger.LogInformation("Import job {JobId} completed: {Imported} imported, {Failed} failed", job.Id, job.ProcessedRows - job.FailedRows, job.FailedRows);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Import job {JobId} failed", job.Id);
            job.Fail(exception.Message, Now());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportRowAsync(ImportJob job, CsvUserRow row, CancellationToken cancellationToken)
    {
        if (row.Request is null)
        {
            job.RecordFailure(row.Row, row.Error!);
            return;
        }

        var result = await _users.CreateAsync(row.Request, cancellationToken);
        if (result.IsSuccessful)
        {
            job.RecordSuccess();
        }
        else
        {
            job.RecordFailure(row.Row, result.ErrorMessage ?? "The row could not be imported.");
        }
    }

    private DateTime Now() => _timeProvider.GetUtcNow().UtcDateTime;
}
