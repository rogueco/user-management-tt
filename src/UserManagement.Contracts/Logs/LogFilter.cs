namespace UserManagement.Contracts.Logs;

public sealed class LogFilter
{
    public long? UserId { get; set; }
    public LogAction? ActionType { get; set; }
    public string? Search { get; set; }
}
