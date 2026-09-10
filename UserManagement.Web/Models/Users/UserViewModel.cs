using System;
using UserManagement.Models;

namespace UserManagement.Web.Models.Users;

public class UserViewModel
{
    public long Id { get; set; }
    public string? Forename { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public bool IsActive { get; set; }

    public static UserViewModel FromUser(User user) => new()
    {
        Id = user.Id,
        Forename = user.Forename,
        Surname = user.Surname,
        Email = user.Email,
        DateOfBirth = user.DateOfBirth,
        IsActive = user.IsActive
    };
}
