using System.Collections.Generic;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Models.Users;

public class UserDetailsViewModel
{
    public UserViewModel User { get; set; } = new();
    public List<LogViewModel> Logs { get; set; } = new();
    public int TotalLogs { get; set; }
}
