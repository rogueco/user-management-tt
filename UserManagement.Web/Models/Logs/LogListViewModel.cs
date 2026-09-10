using System.Collections.Generic;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Web.Models.Logs;

public class LogListViewModel
{
    public List<LogViewModel> Items { get; set; } = new();
    public UserLogFilter Filter { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public Dictionary<string, string> RouteValues(int page)
    {
        var values = new Dictionary<string, string> { ["page"] = page.ToString() };

        if (Filter.UserId is { } userId)
        {
            values["userId"] = userId.ToString();
        }

        if (Filter.ActionType is { } actionType)
        {
            values["actionType"] = actionType.ToString();
        }

        if (!string.IsNullOrWhiteSpace(Filter.Search))
        {
            values["search"] = Filter.Search;
        }

        return values;
    }
}
