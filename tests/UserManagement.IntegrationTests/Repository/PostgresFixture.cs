using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using UserManagement.Repository.Sql;
using UserManagement.Repository.Sql.Configuration;
using UserManagement.Repository.Sql.Seeding;

namespace UserManagement.IntegrationTests.Repository;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public UserManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserManagementDbContext>();
        options.UseUserManagementPostgres(ConnectionString);
        return new UserManagementDbContext(options.Options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await new DatabaseSeeder(db).SeedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(nameof(PostgresCollection))]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
