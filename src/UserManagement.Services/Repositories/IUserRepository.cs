using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;
using UserManagement.Domain.Users;

namespace UserManagement.Services.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<User>> GetPageAsync(UserFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    void Add(User user);
    void Remove(User user);
}
