using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using UserManagement.UnitTests.Fakes;
using UserManagement.Services.Services.ImportServices;
using UserManagement.Services.Services.UserServices;
using UserManagement.Domain.Imports;
using UserManagement.Domain.Users;

namespace UserManagement.UnitTests.Services;

public class ImportProcessorTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly FakeUserLogRepository _logs = new();
    private readonly FakeImportJobRepository _jobs = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ImportProcessor _processor;

    public ImportProcessorTests()
    {
        var cache = new ServiceCollection().AddHybridCache().Services.BuildServiceProvider().GetRequiredService<HybridCache>();
        var userService = new UserService(_users, _logs, _unitOfWork, new FakeTimeProvider(Now), cache, NullLogger<UserService>.Instance);
        _processor = new ImportProcessor(_jobs, userService, _unitOfWork, new FakeTimeProvider(Now), NullLogger<ImportProcessor>.Instance);
    }

    [Fact]
    public async Task ProcessAsync_MustImportGoodRowsAndRecordBadOnes()
    {
        const string csv = """
            Forename,Surname,Email,DateOfBirth,IsActive
            Ada,Lovelace,ada@example.com,1985-12-10,true
            Linus,Torvalds,not-an-email,1969-12-28,true
            Margaret,Hamilton,margaret@example.com,2099-08-17,true
            Ken,Thompson,ken@example.com,1983-02-04,false
            """;
        var job = _jobs.Add("users.csv", csv);

        await _processor.ProcessAsync(job.Id);

        job.Status.Should().Be(ImportStatus.Completed);
        job.TotalRows.Should().Be(4);
        job.ProcessedRows.Should().Be(4);
        job.FailedRows.Should().Be(2);
        job.Errors.Select(e => e.Row).Should().Equal(3, 4);
        job.Errors[0].Message.Should().Contain("Email");
        job.Errors[1].Message.Should().Contain("past");

        _users.Users.Select(u => u.Email).Should().Equal("ada@example.com", "ken@example.com");
        _logs.Logs.Should().HaveCount(2).And.OnlyContain(l => l.Action == UserLogAction.Created);
    }

    [Fact]
    public async Task ProcessAsync_WhenJobIsNotPending_MustDoNothing()
    {
        var job = _jobs.Add("users.csv", "Forename,Surname,Email,DateOfBirth,IsActive\nAda,Lovelace,ada@example.com,1985-12-10,true");
        job.Start(1, Now.UtcDateTime);
        job.Complete(Now.UtcDateTime);

        await _processor.ProcessAsync(job.Id);

        _users.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_WhenJobIsUnknown_MustDoNothing()
    {
        await _processor.ProcessAsync(Guid.NewGuid());

        _unitOfWork.SaveCount.Should().Be(0);
    }
}
