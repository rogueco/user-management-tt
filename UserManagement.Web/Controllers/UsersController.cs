using System.Linq;
using UserManagement.Models;
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

        var model = new UserListViewModel
        {
            Items = _userService.Filter(filter).Select(UserViewModel.FromUser).ToList()
        };

        return View(model);
    }

    [HttpGet("add")]
    public IActionResult Add() => View(new UserFormViewModel());

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(UserFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var user = new User();
        form.ApplyTo(user);
        _userService.Create(user);

        TempData["Message"] = $"{user.Forename} {user.Surname} was added.";
        return RedirectToAction(nameof(List));
    }

    [HttpGet("{id:long}")]
    public IActionResult Details(long id)
    {
        var user = _userService.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(UserViewModel.FromUser(user));
    }

    [HttpGet("{id:long}/edit")]
    public IActionResult Edit(long id)
    {
        var user = _userService.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(UserFormViewModel.FromUser(user));
    }

    [HttpPost("{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, UserFormViewModel form)
    {
        var user = _userService.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        form.ApplyTo(user);
        _userService.Update(user);

        TempData["Message"] = $"{user.Forename} {user.Surname} was updated.";
        return RedirectToAction(nameof(List));
    }

    [HttpGet("{id:long}/delete")]
    public IActionResult Delete(long id)
    {
        var user = _userService.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(UserViewModel.FromUser(user));
    }

    [HttpPost("{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmDelete(long id)
    {
        var user = _userService.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        _userService.Delete(user);

        TempData["Message"] = $"{user.Forename} {user.Surname} was deleted.";
        return RedirectToAction(nameof(List));
    }
}
