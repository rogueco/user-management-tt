using System.Text.Json.Serialization;

namespace UserManagement.Contracts.Logs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogAction
{
    Created,
    Updated,
    Deleted
}

public sealed record LogDto(long Id, long UserId, string UserName, string Email, LogAction Action, DateTime Timestamp);

public sealed record LogChangeDto(string Field, string? Before, string? After);

public sealed record LogDetailsDto(LogDto Log, IReadOnlyList<LogChangeDto> Changes, bool UserExists);
