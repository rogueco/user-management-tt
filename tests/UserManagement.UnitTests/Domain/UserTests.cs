using UserManagement.Domain.Users;

namespace UserManagement.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_MustTrimTextFields()
    {
        var user = User.Create(new("  Peter ", " Loew", " ploew@example.com ", new DateOnly(1968, 11, 11), true));

        user.Details.Should().Be(new UserDetails("Peter", "Loew", "ploew@example.com", new DateOnly(1968, 11, 11), true));
    }

    [Fact]
    public void Update_MustApplyValuesAndReturnWhatChanged()
    {
        var user = User.Create(new("Peter", "Loew", "ploew@example.com", new DateOnly(1968, 11, 11), true));

        var changes = user.Update(new("Peter", "Loew", "peter@example.com", new DateOnly(1968, 11, 11), false));

        user.Email.Should().Be("peter@example.com");
        user.IsActive.Should().BeFalse();
        changes.Should().BeEquivalentTo(new[]
        {
            new UserLogChange("Email", "ploew@example.com", "peter@example.com"),
            new UserLogChange("Account Active", "Yes", "No"),
        });
    }
}
