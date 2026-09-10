using UserManagement.Contracts.Logs;
using UserManagement.Contracts.Users;
using UserManagement.Repository.Sql.Repositories;

namespace UserManagement.IntegrationTests.Repository;

[Collection(nameof(PostgresCollection))]
public class RepositoryTests(PostgresFixture fixture)
{
    [Theory]
    [InlineData(null, 11)]
    [InlineData(true, 7)]
    [InlineData(false, 4)]
    public async Task Users_GetPageAsync_MustApplyActiveFilter(bool? isActive, int expected)
    {
        await using var db = fixture.CreateContext();

        var page = await new UserRepository(db).GetPageAsync(new UserFilter { IsActive = isActive }, 1, 20);

        page.TotalCount.Should().Be(expected);
        page.Items.Should().HaveCount(expected);
    }

    [Fact]
    public async Task Logs_GetPageAsync_MustPageNewestFirstAndClampOutOfRange()
    {
        await using var db = fixture.CreateContext();
        var logs = new UserLogRepository(db);

        var first = await logs.GetPageAsync(new LogFilter(), 1, 20);
        var last = await logs.GetPageAsync(new LogFilter(), 99, 20);

        first.TotalPages.Should().Be(3);
        first.Items.Should().HaveCount(20).And.BeInDescendingOrder(l => l.Timestamp);
        last.Page.Should().Be(3);
        last.Items.Should().HaveCount(7);
    }

    [Fact]
    public async Task Logs_GetPageAsync_MustSearchCaseInsensitively()
    {
        await using var db = fixture.CreateContext();

        var page = await new UserLogRepository(db).GetPageAsync(new LogFilter { Search = "TROY" }, 1, 20);

        page.Items.Should().NotBeEmpty().And.OnlyContain(l => l.Surname == "Troy");
    }

    [Fact]
    public async Task Logs_GetPageAsync_MustFilterByUserAndAction()
    {
        await using var db = fixture.CreateContext();

        var page = await new UserLogRepository(db).GetPageAsync(new LogFilter { UserId = 1001, ActionType = LogAction.Deleted }, 1, 20);

        page.Items.Should().ContainSingle().Which.Forename.Should().Be("Roy");
    }
}
