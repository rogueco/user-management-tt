using UserManagement.Contracts.Users;

namespace UserManagement.UnitTests.Builders;

public class UserDtoBuilder
{
    private long _id = 1;
    private string _forename = "Peter";
    private string _surname = "Loew";
    private string _email = "ploew@example.com";
    private DateOnly _dateOfBirth = new(1968, 11, 11);
    private bool _isActive = true;

    public UserDto Build() => new(_id, _forename, _surname, _email, _dateOfBirth, _isActive);

    public UserDtoBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public UserDtoBuilder WithName(string forename, string surname)
    {
        _forename = forename;
        _surname = surname;
        return this;
    }

    public UserDtoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserDtoBuilder WithDateOfBirth(DateOnly dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public UserDtoBuilder Inactive()
    {
        _isActive = false;
        return this;
    }
}
