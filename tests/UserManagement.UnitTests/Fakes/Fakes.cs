using UserManagement.Services.Repositories;
using UserManagement.Services.Services.ImportServices;
using UserManagement.Services.Services.LogServices;
using UserManagement.Services.Services.UserServices;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Contracts.Users;
using UserManagement.Domain.Imports;
using UserManagement.Domain.Users;

namespace UserManagement.UnitTests.Fakes;

internal sealed class FakeUserRepository : IUserRepository
{
    private long _nextId = 1;

    public List<User> Users { get; } = [];
    public int Reads { get; private set; }

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        Reads++;
        return Task.FromResult(Users.FirstOrDefault(u => u.Id == id));
    }

    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(Users.Any(u => u.Id == id));

    public Task<PagedResult<User>> GetPageAsync(UserFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        Reads++;
        var items = Users.Where(u => filter.IsActive == null || u.IsActive == filter.IsActive).ToList();
        return Task.FromResult(new PagedResult<User>(items, 1, 1, items.Count));
    }

    public void Add(User user)
    {
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, _nextId++);
        Users.Add(user);
    }

    public void Remove(User user) => Users.Remove(user);
}

internal sealed class FakeUserLogRepository : IUserLogRepository
{
    public List<UserLog> Logs { get; } = [];

    public Task<UserLog?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(Logs.FirstOrDefault(l => l.Id == id));

    public Task<PagedResult<UserLog>> GetPageAsync(LogFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<UserLog>(Logs, 1, 1, Logs.Count));

    public void Add(UserLog log) => Logs.Add(log);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);
}

internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class FakeImportJobRepository : IImportJobRepository
{
    public List<ImportJob> Jobs { get; } = [];

    public ImportJob Add(string fileName, string content)
    {
        var job = ImportJob.Create(fileName, content, DateTime.UtcNow);
        Jobs.Add(job);
        return job;
    }

    public Task<ImportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Jobs.FirstOrDefault(j => j.Id == id));

    public Task<IReadOnlyList<ImportJob>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ImportJob>>(Jobs.Take(count).ToList());

    public void Add(ImportJob job) => Jobs.Add(job);
}
