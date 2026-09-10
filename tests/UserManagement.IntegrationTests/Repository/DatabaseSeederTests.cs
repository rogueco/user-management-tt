using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Users;
using UserManagement.Repository.Sql.Seeding;

namespace UserManagement.IntegrationTests.Repository;

[Collection(nameof(PostgresCollection))]
public class DatabaseSeederTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SeedAsync_MustCreateUsersWithHistory()
    {
        await using var db = fixture.CreateContext();

        (await db.Users.CountAsync()).Should().Be(11);
        (await db.UserLogs.CountAsync()).Should().Be(47);
        (await db.UserLogs.CountAsync(l => l.Action == UserLogAction.Deleted)).Should().Be(2);
    }

    [Fact]
    public async Task SeedAsync_MustBeIdempotent()
    {
        await using var db = fixture.CreateContext();
        var before = await db.UserLogs.CountAsync();

        await new DatabaseSeeder(db).SeedAsync();

        (await db.UserLogs.CountAsync()).Should().Be(before);
    }

    [Fact]
    public async Task SeedAsync_MustStoreChangesAsJson()
    {
        await using var db = fixture.CreateContext();

        var log = await db.UserLogs.OrderBy(l => l.Id).FirstAsync();

        log.Action.Should().Be(UserLogAction.Created);
        log.Changes.Should().HaveCount(5);
    }
}
