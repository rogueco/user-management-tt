using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using UserManagement.Services.Results;
using UserManagement.UnitTests.Builders;
using UserManagement.UnitTests.Fakes;
using UserManagement.Services.Services.UserServices;
using UserManagement.Contracts.Users;
using UserManagement.Domain.Users;

namespace UserManagement.UnitTests.Services;

public class UserServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly FakeUserLogRepository _logs = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly UserService _service;

    public UserServiceTests()
        => _service = new UserService(_users, _logs, _unitOfWork, new FakeTimeProvider(Now), CreateCache(), NullLogger<UserService>.Instance);

    // The real in-memory HybridCache, so tag invalidation is exercised rather than faked.
    private static HybridCache CreateCache()
        => new ServiceCollection().AddHybridCache().Services.BuildServiceProvider().GetRequiredService<HybridCache>();

    [Fact]
    public async Task GetByIdAsync_MustServeRepeatReadsFromCache()
    {
        await _service.CreateAsync(Request());
        var readsAfterCreate = _users.Reads;

        await _service.GetByIdAsync(1);
        await _service.GetByIdAsync(1);

        _users.Reads.Should().Be(readsAfterCreate + 1);
    }

    [Fact]
    public async Task GetPageAsync_MustNotBeStaleAfterAWrite()
    {
        await _service.CreateAsync(Request());
        (await _service.GetPageAsync(new UserFilter(), 1, 20)).Value!.TotalCount.Should().Be(1);

        await _service.CreateAsync(Request(email: "second@example.com"));
        (await _service.GetPageAsync(new UserFilter(), 1, 20)).Value!.TotalCount.Should().Be(2);

        await _service.UpdateAsync(1, Request(surname: "Renamed"));
        (await _service.GetByIdAsync(1)).Value!.Surname.Should().Be("Renamed");

        await _service.DeleteAsync(1);
        (await _service.GetByIdAsync(1)).ResultCode.Should().Be(ServiceResultCode.NotFound);
        (await _service.GetPageAsync(new UserFilter(), 1, 20)).Value!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_MustStoreUserAndRecordCreation()
    {
        var created = await _service.CreateAsync(Request());

        created.IsSuccessful.Should().BeTrue();
        created.Value!.Id.Should().Be(1);
        _users.Users.Should().ContainSingle().Which.Email.Should().Be("testy@example.com");

        var log = _logs.Logs.Should().ContainSingle().Subject;
        log.Action.Should().Be(UserLogAction.Created);
        log.UserId.Should().Be(1);
        log.Timestamp.Should().Be(Now.UtcDateTime);
        log.Changes.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsInvalid_MustReturnInvalidInputWithoutStoring()
    {
        var result = await _service.CreateAsync(Request(email: "not-an-email"));

        result.ResultCode.Should().Be(ServiceResultCode.InvalidInput);
        result.Errors.Should().ContainKey("Email");
        _users.Users.Should().BeEmpty();
        _logs.Logs.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_MustApplyChangesAndRecordOnlyWhatChanged()
    {
        await _service.CreateAsync(Request());

        var updated = await _service.UpdateAsync(1, Request(surname: "Renamed"));

        updated.Value!.Surname.Should().Be("Renamed");
        var log = _logs.Logs.Last();
        log.Action.Should().Be(UserLogAction.Updated);
        log.Changes.Should().ContainSingle().Which.Should().Be(new UserLogChange("Surname", "McTest", "Renamed"));
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_MustReturnNotFoundAndRecordNothing()
    {
        var updated = await _service.UpdateAsync(999, Request());

        updated.ResultCode.Should().Be(ServiceResultCode.NotFound);
        _logs.Logs.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_MustRemoveUserAndRecordDeletion()
    {
        await _service.CreateAsync(Request());

        var deleted = await _service.DeleteAsync(1);

        deleted.IsSuccessful.Should().BeTrue();
        _users.Users.Should().BeEmpty();
        _logs.Logs.Last().Action.Should().Be(UserLogAction.Deleted);
        _logs.Logs.Last().Changes.Should().OnlyContain(c => c.After == null);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_MustReturnNotFound()
    {
        (await _service.DeleteAsync(999)).ResultCode.Should().Be(ServiceResultCode.NotFound);
    }

    private static UserRequest Request(string surname = "McTest", string email = "testy@example.com")
        => new UserRequestBuilder().WithSurname(surname).WithEmail(email).Build();
}
