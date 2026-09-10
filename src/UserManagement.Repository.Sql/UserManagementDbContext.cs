using System.Diagnostics.CodeAnalysis;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserManagement.Services.Repositories;
using UserManagement.Domain.Imports;
using UserManagement.Domain.Users;

namespace UserManagement.Repository.Sql;

[ExcludeFromCodeCoverage]
public sealed class UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserLog> UserLogs => Set<UserLog>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserManagementDbContext).Assembly);

        // MassTransit transactional outbox and inbox tables.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        // Inside a message consumer the inbox already owns a transaction; join it rather than nest one.
        if (Database.CurrentTransaction is not null)
        {
            return action(cancellationToken);
        }

        return Database.CreateExecutionStrategy().ExecuteAsync(async ct =>
        {
            await using var transaction = await Database.BeginTransactionAsync(ct);
            var result = await action(ct);
            await transaction.CommitAsync(ct);
            return result;
        }, cancellationToken);
    }
}
