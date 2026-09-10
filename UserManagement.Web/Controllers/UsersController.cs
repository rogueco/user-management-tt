using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Domain.Models;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public IActionResult List([FromQuery] UserFilter filter)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(List));
        }

        var items = _userService.Filter(filter).Select(p => new UserListItemViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            DateOfBirth = p.DateOfBirth,
            IsActive = p.IsActive
        });

        var model = new UserListViewModel
        {
            Items = items.ToList()
        };

        return View(model);
    }
}
