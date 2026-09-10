using Microsoft.EntityFrameworkCore;
using UserManagement.Repository.Sql.Extensions;
using UserManagement.Services.Repositories;
using UserManagement.Services.Services.LogServices;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Domain.Users;

namespace UserManagement.Repository.Sql.Repositories;

internal sealed class UserLogRepository : IUserLogRepository
{
    private readonly UserManagementDbContext _db;

    public UserLogRepository(UserManagementDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task<UserLog?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _db.UserLogs.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<PagedResult<UserLog>> GetPageAsync(LogFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.UserLogs.AsQueryable();

        if (filter.UserId is { } userId)
        {
            query = query.Where(l => l.UserId == userId);
        }

        if (filter.ActionType is { } actionType)
        {
            var action = actionType.ToDomain();
            query = query.Where(l => l.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Forename, pattern)
                || EF.Functions.ILike(l.Surname, pattern)
                || EF.Functions.ILike(l.Email, pattern));
        }

        return query
            .OrderByDescending(l => l.Timestamp)
            .ThenByDescending(l => l.Id)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public void Add(UserLog log) => _db.UserLogs.Add(log);
}
