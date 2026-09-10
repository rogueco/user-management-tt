namespace UserManagement.Domain.Users;

public class UserLog
{
    private UserLog()
    {
    }

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string Forename { get; private set; } = default!;
    public string Surname { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public UserLogAction Action { get; private set; }
    public DateTime Timestamp { get; private set; }
    public IReadOnlyList<UserLogChange> Changes { get; private set; } = [];

    public static UserLog Create(UserLogAction action, long userId, UserDetails snapshot, IReadOnlyList<UserLogChange> changes, DateTime timestamp) => new()
    {
        UserId = userId,
        Forename = snapshot.Forename,
        Surname = snapshot.Surname,
        Email = snapshot.Email,
        Action = action,
        Timestamp = timestamp,
        Changes = changes
    };
}
