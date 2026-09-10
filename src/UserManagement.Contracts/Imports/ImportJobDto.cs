using System.Text.Json.Serialization;

namespace UserManagement.Contracts.Imports;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImportJobStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public sealed record ImportErrorDto(int Row, string Message);

public sealed record ImportJobDto(
    Guid Id,
    string FileName,
    ImportJobStatus Status,
    int TotalRows,
    int ProcessedRows,
    int FailedRows,
    IReadOnlyList<ImportErrorDto> Errors,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);
