using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UserManagement.Models;

namespace UserManagement.Web.Models.Users;

public class UserFormViewModel : IValidatableObject
{
    [Required, StringLength(50)]
    public string? Forename { get; set; }

    [Required, StringLength(50)]
    public string? Surname { get; set; }

    [Required, EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "Account Active")]
    public bool IsActive { get; set; }

    public static UserFormViewModel FromUser(User user) => new()
    {
        Forename = user.Forename,
        Surname = user.Surname,
        Email = user.Email,
        DateOfBirth = user.DateOfBirth,
        IsActive = user.IsActive
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult("Date of Birth must be in the past.", [nameof(DateOfBirth)]);
        }
    }

    /// <summary>
    /// Copies the form values onto the user. Only valid once validation has passed.
    /// </summary>
    public void ApplyTo(User user)
    {
        user.Forename = Forename!.Trim();
        user.Surname = Surname!.Trim();
        user.Email = Email!.Trim();
        user.DateOfBirth = DateOfBirth!.Value;
        user.IsActive = IsActive;
    }
}
