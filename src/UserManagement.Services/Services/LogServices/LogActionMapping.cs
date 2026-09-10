using UserManagement.Contracts.Logs;
using UserManagement.Domain.Users;

namespace UserManagement.Services.Services.LogServices;

public static class LogActionMapping
{
    public static UserLogAction ToDomain(this LogAction action) => action switch
    {
        LogAction.Created => UserLogAction.Created,
        LogAction.Updated => UserLogAction.Updated,
        LogAction.Deleted => UserLogAction.Deleted,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };

    public static LogAction ToContract(this UserLogAction action) => action switch
    {
        UserLogAction.Created => LogAction.Created,
        UserLogAction.Updated => LogAction.Updated,
        UserLogAction.Deleted => LogAction.Deleted,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}
