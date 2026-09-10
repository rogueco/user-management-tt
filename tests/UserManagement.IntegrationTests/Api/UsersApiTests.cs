using System.Net;
using System.Net.Http.Json;
using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Contracts.Users;

namespace UserManagement.IntegrationTests.Api;

public class UsersApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private HttpClient Client => fixture.Client;

    [Fact]
    public async Task GetUsers_MustReturnSeededUsers()
    {
        var page = await Client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users");

        page!.TotalCount.Should().Be(11);
        page.Items.Should().HaveCount(11);
    }

    [Fact]
    public async Task GetUsers_MustApplyActiveFilter()
    {
        var page = await Client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users?isActive=false");

        page!.Items.Should().HaveCount(4).And.OnlyContain(u => !u.IsActive);
    }

    [Fact]
    public async Task GetUser_WhenUnknown_MustReturnNotFound()
    {
        var response = await Client.GetAsync("/api/v1/users/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WhenFieldsAreInvalid_MustReturnValidationProblem()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/users", new UserRequest { Forename = "", Email = "nope" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Forename").And.Contain("Surname").And.Contain("Email").And.Contain("DateOfBirth");
    }

    [Fact]
    public async Task CreateUser_WhenDateOfBirthIsNotInThePast_MustReturnValidationProblem()
    {
        var request = new UserRequest { Forename = "A", Surname = "B", Email = "a@b.com", DateOfBirth = new DateOnly(2099, 1, 1) };

        var response = await Client.PostAsJsonAsync("/api/v1/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("must be in the past");
    }

    [Fact]
    public async Task CreateUpdateDelete_MustRoundTripAndBeLogged()
    {
        var request = new UserRequest { Forename = "Testy", Surname = "McTest", Email = "testy@example.com", DateOfBirth = new DateOnly(1990, 6, 15), IsActive = true };

        var created = await Client.PostAsJsonAsync("/api/v1/users", request);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = (await created.Content.ReadFromJsonAsync<UserDto>())!;
        created.Headers.Location!.ToString().Should().EndWithEquivalentOf($"/api/v1/users/{user.Id}");

        request.Surname = "Renamed";
        var updated = await Client.PutAsJsonAsync($"/api/v1/users/{user.Id}", request);
        updated.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleted = await Client.DeleteAsync($"/api/v1/users/{user.Id}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Client.GetAsync($"/api/v1/users/{user.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var logs = await Client.GetFromJsonAsync<PagedResult<LogDto>>($"/api/v1/logs?userId={user.Id}");
        logs!.Items.Select(l => l.Action).Should().Equal(LogAction.Deleted, LogAction.Updated, LogAction.Created);

        var details = await Client.GetFromJsonAsync<LogDetailsDto>($"/api/v1/logs/{logs.Items[1].Id}");
        details!.Changes.Should().ContainSingle().Which.Should().Be(new LogChangeDto("Surname", "McTest", "Renamed"));
        details.UserExists.Should().BeFalse();
    }

    [Fact]
    public async Task GetLogs_MustPageAndFilter()
    {
        var page = await Client.GetFromJsonAsync<PagedResult<LogDto>>("/api/v1/logs?actionType=Created&pageSize=5");

        page!.Items.Should().HaveCount(5).And.OnlyContain(l => l.Action == LogAction.Created);
        page.TotalPages.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Health_MustReportReady()
    {
        (await Client.GetAsync("/health/live")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.GetAsync("/health/ready")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
