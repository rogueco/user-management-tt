using System.Globalization;

namespace UserManagement.Domain.Users;

public enum UserLogAction
{
    Created,
    Updated,
    Deleted
}

public sealed record UserLogChange(string Field, string? Before, string? After)
{
    public static IReadOnlyList<UserLogChange> Between(UserDetails? before, UserDetails? after)
    {
        var changes = new List<UserLogChange>();

        Compare("Forename", before?.Forename, after?.Forename);
        Compare("Surname", before?.Surname, after?.Surname);
        Compare("Email", before?.Email, after?.Email);
        Compare("Date of Birth", DateOfBirth(before), DateOfBirth(after));
        Compare("Account Active", ActiveState(before), ActiveState(after));

        return changes;

        void Compare(string field, string? oldValue, string? newValue)
        {
            if (oldValue != newValue)
            {
                changes.Add(new UserLogChange(field, oldValue, newValue));
            }
        }

        static string? DateOfBirth(UserDetails? details)
            => details?.DateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        static string? ActiveState(UserDetails? details)
            => details is null ? null : details.IsActive ? "Yes" : "No";
    }
}
