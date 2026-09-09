namespace UserManagement.Services.Domain.Models;

/// <summary>
/// Filter criteria for user queries. Unset properties are ignored, so new criteria
/// can be added without changing the controller or service signatures.
/// </summary>
public class UserFilter
{
    /// <summary>
    /// When set, restricts results to users with a matching active state. Null returns all users.
    /// </summary>
    public bool? IsActive { get; set; }
}
