using System;
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


    public static ModelBuilder SeedUsers(this ModelBuilder model)
    {
        var random = new Random(DateOfBirthSeed);
        DateOnly Dob() => random.NextDateOnly(Earliest, Latest);

        model.Entity<User>().HasData(new[]
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
        });

        return model;
    }

    private static DateOnly NextDateOnly(this Random random, DateOnly from, DateOnly to) => from.AddDays(random.Next(to.DayNumber - from.DayNumber + 1));
}
