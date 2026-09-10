using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data;

public static class ModelBuilderExtensions
{
    // Fixed seed data. this ensures that tests all see identical data.
    // Only the constant makes the "random" dates repeatable.
    private const int DateOfBirthSeed = 1967;
    private static readonly DateOnly Earliest = new(1950, 1, 1);
    private static readonly DateOnly Latest = new(2005, 12, 31);
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    public static ModelBuilder SeedUsers(this ModelBuilder model)
    {
        var random = new Random(DateOfBirthSeed);
        DateOnly Dob() => random.NextDateOnly(Earliest, Latest);

        var users = new[]
        {
            new User { Id = 1, Forename = "Peter", Surname = "Loew", Email = "ploew@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", Email = "bfgates@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", Email = "ctroy@example.com", DateOfBirth = Dob(), IsActive = false },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", Email = "mraines@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", Email = "sgodspeed@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", Email = "himcdunnough@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", Email = "cpoe@example.com", DateOfBirth = Dob(), IsActive = false },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", Email = "emalus@example.com", DateOfBirth = Dob(), IsActive = false },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", Email = "dmacready@example.com", DateOfBirth = Dob(), IsActive = false },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", Email = "jblaze@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", Email = "rfeld@example.com", DateOfBirth = Dob(), IsActive = true },
        };

        // Users that were later deleted. Their log entries remain, which is why UserLog has no foreign key.
        var deletedUsers = new[]
        {
            new User { Id = 1001, Forename = "Roy", Surname = "Waller", Email = "rwaller@example.com", DateOfBirth = Dob(), IsActive = true },
            new User { Id = 1002, Forename = "Terence", Surname = "McDonagh", Email = "tmcdonagh@example.com", DateOfBirth = Dob(), IsActive = false },
        };

        model.Entity<User>().HasData(users);
        model.Entity<UserLog>().HasData(SeedLogs(users, deletedUsers, random));

        return model;
    }

    // A history for every seeded user: created, then a few corrections ending at the user's current values.
    private static List<UserLog> SeedLogs(User[] users, User[] deletedUsers, Random random)
    {
        var corrections = new Func<User, User>[]
        {
            user => Copy(user, u => u.DateOfBirth = user.DateOfBirth.AddYears(-1)),
            user => Copy(user, u => u.Email = user.Email.Replace("@example.com", "@example.org")),
            user => Copy(user, u => u.IsActive = !user.IsActive),
            user => Copy(user, u => u.Surname = user.Surname.ToUpperInvariant()),
        };

        var everyone = users.Concat(deletedUsers).ToArray();
        var logs = new List<UserLog>();
        var timestamp = SeedTimestamp;

        void Add(UserLogAction action, User user, IReadOnlyList<UserLogChange> changes)
        {
            timestamp = timestamp.AddHours(random.Next(1, 36));
            var log = new UserLog
            {
                Id = logs.Count + 1,
                UserId = user.Id,
                Forename = user.Forename,
                Surname = user.Surname,
                Email = user.Email,
                Action = action,
                Timestamp = timestamp
            };
            log.SetChanges(changes);
            logs.Add(log);
        }

        foreach (var user in everyone)
        {
            Add(UserLogAction.Created, user, UserLogChange.Between(null, user));
        }

        foreach (var user in everyone)
        {
            var count = random.Next(1, corrections.Length + 1);
            foreach (var correction in corrections.OrderBy(_ => random.Next()).Take(count))
            {
                Add(UserLogAction.Updated, user, UserLogChange.Between(correction(user), user));
            }
        }

        foreach (var user in deletedUsers)
        {
            Add(UserLogAction.Deleted, user, UserLogChange.Between(user, null));
        }

        return logs;
    }

    private static User Copy(User user, Action<User> change)
    {
        var copy = new User
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };
        change(copy);
        return copy;
    }

    private static DateOnly NextDateOnly(this Random random, DateOnly from, DateOnly to) => from.AddDays(random.Next(to.DayNumber - from.DayNumber + 1));
}
