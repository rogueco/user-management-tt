using System;
using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Web.Models.Logs;

public class LogViewModel
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public UserLogAction Action { get; set; }
    public DateTime Timestamp { get; set; }
    public IReadOnlyList<UserLogChange> Changes { get; set; } = [];
    public bool UserExists { get; set; }

    public static LogViewModel FromLog(UserLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserName = $"{log.Forename} {log.Surname}",
        Email = log.Email,
        Action = log.Action,
        Timestamp = log.Timestamp,
        Changes = log.GetChanges()
    };
}
