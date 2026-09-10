using System.ComponentModel.DataAnnotations;

namespace UserManagement.Contracts.Users;

public sealed class UserRequest : IValidatableObject
{
    [Required, StringLength(50)]
    public string Forename { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Surname { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "Account Active")]
    public bool IsActive { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult("Date of Birth must be in the past.", [nameof(DateOfBirth)]);
        }
    }
}
