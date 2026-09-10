namespace UserManagement.Domain.Users;

public class User
{
    private User()
    {
    }

    public long Id { get; private set; }
    public string Forename { get; private set; } = default!;
    public string Surname { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateOnly DateOfBirth { get; private set; }
    public bool IsActive { get; private set; }

    public UserDetails Details => new(Forename, Surname, Email, DateOfBirth, IsActive);

    public static User Create(UserDetails details)
    {
        var user = new User();
        user.Apply(details);
        return user;
    }

    public IReadOnlyList<UserLogChange> Update(UserDetails details)
    {
        var changes = UserLogChange.Between(Details, details);
        Apply(details);
        return changes;
    }

    private void Apply(UserDetails details)
    {
        Forename = details.Forename.Trim();
        Surname = details.Surname.Trim();
        Email = details.Email.Trim();
        DateOfBirth = details.DateOfBirth;
        IsActive = details.IsActive;
    }
}
