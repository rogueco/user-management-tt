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
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

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
}
