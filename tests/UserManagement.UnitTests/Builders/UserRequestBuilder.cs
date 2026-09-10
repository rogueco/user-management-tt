using UserManagement.Contracts.Users;

namespace UserManagement.UnitTests.Builders;

public class UserRequestBuilder
{
    private readonly UserRequest _request = new()
    {
        Forename = "Testy",
        Surname = "McTest",
        Email = "testy@example.com",
        DateOfBirth = new DateOnly(1990, 6, 15),
        IsActive = true
    };

    public UserRequest Build() => _request;

    public UserRequestBuilder WithForename(string forename)
    {
        _request.Forename = forename;
        return this;
    }

    public UserRequestBuilder WithSurname(string surname)
    {
        _request.Surname = surname;
        return this;
    }

    public UserRequestBuilder WithEmail(string email)
    {
        _request.Email = email;
        return this;
    }

    public UserRequestBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        _request.DateOfBirth = dateOfBirth;
        return this;
    }

    public UserRequestBuilder Inactive()
    {
        _request.IsActive = false;
        return this;
    }
}
