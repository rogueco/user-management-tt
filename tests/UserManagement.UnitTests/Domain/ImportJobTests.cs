using UserManagement.Domain.Imports;

namespace UserManagement.UnitTests.Domain;

public class ImportJobTests
{
    private static readonly DateTime Now = new(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_MustStartPending()
    {
        var job = ImportJob.Create("users.csv", "content", Now);

        job.Status.Should().Be(ImportStatus.Pending);
        job.CreatedAt.Should().Be(Now);
        job.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Lifecycle_MustCountRowsAndCollectErrors()
    {
        var job = ImportJob.Create("users.csv", "content", Now);

        job.Start(3, Now);
        job.RecordSuccess();
        job.RecordFailure(3, "Email is invalid");
        job.RecordSuccess();
        job.Complete(Now.AddSeconds(5));

        job.Status.Should().Be(ImportStatus.Completed);
        job.TotalRows.Should().Be(3);
        job.ProcessedRows.Should().Be(3);
        job.FailedRows.Should().Be(1);
        job.Errors.Should().ContainSingle().Which.Should().Be(new ImportError(3, "Email is invalid"));
        job.CompletedAt.Should().Be(Now.AddSeconds(5));
    }

    [Fact]
    public void Fail_MustRecordTheReason()
    {
        var job = ImportJob.Create("users.csv", "content", Now);
        job.Start(1, Now);

        job.Fail("File could not be read", Now);

        job.Status.Should().Be(ImportStatus.Failed);
        job.Errors.Should().ContainSingle().Which.Message.Should().Be("File could not be read");
    }
}
