using UserManagement.Domain.Users;

namespace UserManagement.UnitTests.Domain;

public class UserLogChangeTests
{
    private static readonly UserDetails Details = new("Peter", "Loew", "ploew@example.com", new DateOnly(1968, 11, 11), true);

    [Fact]
    public void Between_WhenCreated_MustListEveryField()
    {
        var changes = UserLogChange.Between(null, Details);

        changes.Should().HaveCount(5);
        changes.Should().OnlyContain(c => c.Before == null && c.After != null);
        changes.Should().ContainSingle(c => c.Field == "Date of Birth").Which.After.Should().Be("11/11/1968");
        changes.Should().ContainSingle(c => c.Field == "Account Active").Which.After.Should().Be("Yes");
    }

    [Fact]
    public void Between_WhenDeleted_MustListEveryField()
    {
        var changes = UserLogChange.Between(Details, null);

        changes.Should().HaveCount(5);
        changes.Should().OnlyContain(c => c.Before != null && c.After == null);
    }

    [Fact]
    public void Between_WhenOneFieldDiffers_MustListOnlyThatField()
    {
        var changes = UserLogChange.Between(Details, Details with { Surname = "Renamed" });

        changes.Should().ContainSingle().Which.Should().Be(new UserLogChange("Surname", "Loew", "Renamed"));
    }

    [Fact]
    public void Between_WhenNothingDiffers_MustBeEmpty()
    {
        UserLogChange.Between(Details, Details).Should().BeEmpty();
    }

    [Fact]
    public void Between_MustCoverEveryUserDetailsProperty()
    {
        // Guards against a new field being added to UserDetails and silently missing from the audit trail.
        var properties = typeof(UserDetails).GetProperties().Length;

        UserLogChange.Between(null, Details).Should().HaveCount(properties);
    }
}
