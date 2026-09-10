using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Domain.Models;
using UserManagement.Web.Models.Logs;

namespace UserManagement.WebMS.Controllers;

[Route("logs")]
public class LogsController : Controller
{
    private const int PageSize = 20;

    private readonly IUserLogService _userLogService;
    private readonly IUserService _userService;

    public LogsController(IUserLogService userLogService, IUserService userService)
    {
        _userLogService = userLogService;
        _userService = userService;
    }

    [HttpGet]
    public IActionResult List([FromQuery] UserLogFilter filter, int page = 1)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(List));
        }

        var result = _userLogService.Filter(filter, page, PageSize);

        return View(new LogListViewModel
        {
            Items = result.Items.Select(LogViewModel.FromLog).ToList(),
            Filter = filter,
            Page = result.Page,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id:long}")]
    public IActionResult Details(long id)
    {
        var log = _userLogService.GetById(id);
        if (log is null)
        {
            return NotFound();
        }

        var model = LogViewModel.FromLog(log);
        model.UserExists = _userService.GetById(log.UserId) is not null;

        return View(model);
    }
}
