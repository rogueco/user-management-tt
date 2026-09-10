using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Domain.Implementations;

public class UserLogService : IUserLogService
{
    private readonly IDataContext _dataAccess;
    private readonly TimeProvider _timeProvider;

    public UserLogService(IDataContext dataAccess, TimeProvider timeProvider)
    {
        _dataAccess = dataAccess;
        _timeProvider = timeProvider;
    }

    public void Record(UserLogAction action, User snapshot, IReadOnlyList<UserLogChange> changes)
    {
        var log = new UserLog
        {
            UserId = snapshot.Id,
            Forename = snapshot.Forename,
            Surname = snapshot.Surname,
            Email = snapshot.Email,
            Action = action,
            Timestamp = _timeProvider.GetUtcNow().UtcDateTime
        };
        log.SetChanges(changes);

        _dataAccess.Create(log);
    }

    public PagedResult<UserLog> Filter(UserLogFilter filter, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dataAccess.GetAll<UserLog>();

        if (filter.UserId is { } userId)
        {
            query = query.Where(l => l.UserId == userId);
        }

        if (filter.ActionType is { } actionType)
        {
            query = query.Where(l => l.Action == actionType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(l => l.Forename.ToLower().Contains(search)
                || l.Surname.ToLower().Contains(search)
                || l.Email.ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1);
        page = Math.Min(page, totalPages);

        return new PagedResult<UserLog>
        {
            Items = query
                .OrderByDescending(l => l.Timestamp)
                .ThenByDescending(l => l.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };
    }

    public UserLog? GetById(long id) => _dataAccess.GetAll<UserLog>().FirstOrDefault(l => l.Id == id);
}
