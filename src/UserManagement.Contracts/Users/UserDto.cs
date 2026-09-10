namespace UserManagement.Contracts.Users;

public sealed record UserDto(long Id, string Forename, string Surname, string Email, DateOnly DateOfBirth, bool IsActive);
