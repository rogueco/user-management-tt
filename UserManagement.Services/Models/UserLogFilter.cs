using UserManagement.Models;

namespace UserManagement.Services.Domain.Models;

public class UserLogFilter
{
    public long? UserId { get; set; }
    public UserLogAction? ActionType { get; set; }
    public string? Search { get; set; }
}
