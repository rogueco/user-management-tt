using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Domain.Users;

namespace UserManagement.Services.Repositories;

public interface IUserLogRepository
{
    Task<UserLog?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<UserLog>> GetPageAsync(LogFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    void Add(UserLog log);
}
