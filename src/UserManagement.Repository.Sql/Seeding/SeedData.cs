using UserManagement.Domain.Users;

namespace UserManagement.Repository.Sql.Seeding;

// Deterministic: the fixed seed means every environment gets identical data.
internal sealed class SeedData
{
    private const int Seed = 1967;
    private static readonly DateOnly EarliestDateOfBirth = new(1950, 1, 1);
    private static readonly DateOnly LatestDateOfBirth = new(2005, 12, 31);
    private static readonly DateTime FirstLogTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Random _random = new(Seed);

    public IReadOnlyList<User> Users() =>
    [
        User.Create(new("Peter", "Loew", "ploew@example.com", DateOfBirth(), true)),
        User.Create(new("Benjamin Franklin", "Gates", "bfgates@example.com", DateOfBirth(), true)),
        User.Create(new("Castor", "Troy", "ctroy@example.com", DateOfBirth(), false)),
        User.Create(new("Memphis", "Raines", "mraines@example.com", DateOfBirth(), true)),
        User.Create(new("Stanley", "Goodspeed", "sgodspeed@example.com", DateOfBirth(), true)),
        User.Create(new("H.I.", "McDunnough", "himcdunnough@example.com", DateOfBirth(), true)),
        User.Create(new("Cameron", "Poe", "cpoe@example.com", DateOfBirth(), false)),
        User.Create(new("Edward", "Malus", "emalus@example.com", DateOfBirth(), false)),
        User.Create(new("Damon", "Macready", "dmacready@example.com", DateOfBirth(), false)),
        User.Create(new("Johnny", "Blaze", "jblaze@example.com", DateOfBirth(), true)),
        User.Create(new("Robin", "Feld", "rfeld@example.com", DateOfBirth(), true)),
    ];

    // A history for every user: created, then a few corrections ending at the current values.
    // Two users that were later deleted are included; their entries outlive them.
    public IReadOnlyList<UserLog> Logs(IReadOnlyList<User> users)
    {
        var deleted = new (long Id, UserDetails Details)[]
        {
            (1001, new("Roy", "Waller", "rwaller@example.com", DateOfBirth(), true)),
            (1002, new("Terence", "McDonagh", "tmcdonagh@example.com", DateOfBirth(), false)),
        };

        var corrections = new Func<UserDetails, UserDetails>[]
        {
            d => d with { DateOfBirth = d.DateOfBirth.AddYears(-1) },
            d => d with { Email = d.Email.Replace("@example.com", "@example.org") },
            d => d with { IsActive = !d.IsActive },
            d => d with { Surname = d.Surname.ToUpperInvariant() },
        };

        var everyone = users.Select(u => (u.Id, u.Details)).Concat(deleted).ToList();
        var logs = new List<UserLog>();
        var timestamp = FirstLogTimestamp;

        void Add(UserLogAction action, long userId, UserDetails details, IReadOnlyList<UserLogChange> changes)
        {
            timestamp = timestamp.AddHours(_random.Next(1, 36));
            logs.Add(UserLog.Create(action, userId, details, changes, timestamp));
        }

        foreach (var (id, details) in everyone)
        {
            Add(UserLogAction.Created, id, details, UserLogChange.Between(null, details));
        }

        foreach (var (id, details) in everyone)
        {
            var count = _random.Next(1, corrections.Length + 1);
            foreach (var correction in corrections.OrderBy(_ => _random.Next()).Take(count))
            {
                Add(UserLogAction.Updated, id, details, UserLogChange.Between(correction(details), details));
            }
        }

        foreach (var (id, details) in deleted)
        {
            Add(UserLogAction.Deleted, id, details, UserLogChange.Between(details, null));
        }

        return logs;
    }

    private DateOnly DateOfBirth()
        => EarliestDateOfBirth.AddDays(_random.Next(LatestDateOfBirth.DayNumber - EarliestDateOfBirth.DayNumber + 1));
}
