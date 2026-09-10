namespace UserManagement.Domain.Users;

public sealed record UserDetails(string Forename, string Surname, string Email, DateOnly DateOfBirth, bool IsActive);
