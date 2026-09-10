using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    private readonly IUserLogService _userLogService;

    public UserService(IDataContext dataAccess, IUserLogService userLogService)
    {
        _dataAccess = dataAccess;
        _userLogService = userLogService;
    }

    /// <summary>
    /// Return users matching filter. Unset properties are ignored
    /// </summary>
    /// <param name="filter">The criteria to apply. An empty filter returns all users</param>
    /// <returns>The users matching every set property on <paramref name="filter"/></returns>
    public IEnumerable<User> Filter(UserFilter filter)
    {
        var query = _dataAccess.GetAll<User>();

        if (filter.IsActive is { } isActive)
        {
            query = query.Where(x => x.IsActive == isActive);
        }

        return query;
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public User? GetById(long id) => _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == id);

    public void Create(User user)
    {
        _dataAccess.Create(user);
        _userLogService.Record(UserLogAction.Created, user, UserLogChange.Between(null, user));
    }

    public void Update(User user)
    {
        var existing = GetById(user.Id) ?? throw new KeyNotFoundException($"User {user.Id} does not exist.");
        var changes = UserLogChange.Between(existing, user);

        existing.Forename = user.Forename;
        existing.Surname = user.Surname;
        existing.Email = user.Email;
        existing.DateOfBirth = user.DateOfBirth;
        existing.IsActive = user.IsActive;

        _dataAccess.Update(existing);
        _userLogService.Record(UserLogAction.Updated, existing, changes);
    }

    public void Delete(User user)
    {
        _dataAccess.Delete(user);
        _userLogService.Record(UserLogAction.Deleted, user, UserLogChange.Between(user, null));
    }
}
