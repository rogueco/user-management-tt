using System.Collections.Generic;
using UserManagement.Models;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Return users matching filter. Unset properties are ignored
    /// </summary>
    /// <param name="filter">The criteria to apply. An empty filter returns all users</param>
    /// <returns>The users matching every set property on <paramref name="filter"/></returns>
    IEnumerable<User> Filter(UserFilter filter);
    IEnumerable<User> GetAll();
}
