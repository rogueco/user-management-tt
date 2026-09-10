using System.Collections.Generic;
using System.Globalization;

namespace UserManagement.Models;

public enum UserLogAction
{
    Created,
    Updated,
    Deleted
}

public record UserLogChange(string Field, string? Before, string? After)
{
    public static IReadOnlyList<UserLogChange> Between(User? before, User? after)
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

        static string? DateOfBirth(User? user)
            => user?.DateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        static string? ActiveState(User? user)
            => user is null ? null : user.IsActive ? "Yes" : "No";
    }
}
