using Microsoft.EntityFrameworkCore;
using UserManagement.Repository.Sql.Extensions;
using UserManagement.Services.Repositories;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;
using UserManagement.Domain.Users;

namespace UserManagement.Repository.Sql.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly UserManagementDbContext _db;

    public UserRepository(UserManagementDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
        => _db.Users.AnyAsync(u => u.Id == id, cancellationToken);

    public Task<PagedResult<User>> GetPageAsync(UserFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.AsQueryable();

        if (filter.IsActive is { } isActive)
        {
            query = query.Where(u => u.IsActive == isActive);
        }

        return query.OrderBy(u => u.Id).ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public void Add(User user) => _db.Users.Add(user);

    public void Remove(User user) => _db.Users.Remove(user);
}
