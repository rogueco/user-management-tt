namespace UserManagement.Domain.Imports;

public enum ImportStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public sealed record ImportError(int Row, string Message);

public class ImportJob
{
    private ImportJob()
    {
    }

    public Guid Id { get; private set; }
    public string FileName { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public ImportStatus Status { get; private set; }
    public int TotalRows { get; private set; }
    public int ProcessedRows { get; private set; }
    public int FailedRows { get; private set; }
    public IReadOnlyList<ImportError> Errors { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public static ImportJob Create(string fileName, string content, DateTime now) => new()
    {
        Id = Guid.CreateVersion7(),
        FileName = fileName,
        Content = content,
        Status = ImportStatus.Pending,
        CreatedAt = now
    };

    public void Start(int totalRows, DateTime now)
    {
        Status = ImportStatus.Running;
        TotalRows = totalRows;
        StartedAt = now;
    }

    public void RecordSuccess() => ProcessedRows++;

    public void RecordFailure(int row, string message)
    {
        ProcessedRows++;
        FailedRows++;
        Errors = [.. Errors, new ImportError(row, message)];
    }

    public void Complete(DateTime now)
    {
        Status = ImportStatus.Completed;
        CompletedAt = now;
    }

    public void Fail(string reason, DateTime now)
    {
        Status = ImportStatus.Failed;
        Errors = [.. Errors, new ImportError(0, reason)];
        CompletedAt = now;
    }
}
