using System.Collections.Generic;
using UserManagement.Models;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserLogService
{
    /// <summary>
    /// Record an action performed on a user. The snapshot is the user's state at the time of the action
    /// </summary>
    void Record(UserLogAction action, User snapshot, IReadOnlyList<UserLogChange> changes);

    /// <summary>
    /// Return a page of log entries matching the filter, newest first. Unset filter properties are ignored
    /// </summary>
    PagedResult<UserLog> Filter(UserLogFilter filter, int page, int pageSize);

    UserLog? GetById(long id);
}
