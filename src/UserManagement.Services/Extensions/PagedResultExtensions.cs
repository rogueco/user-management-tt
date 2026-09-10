using UserManagement.Contracts.Common;

namespace UserManagement.Services.Extensions;

public static class PagedResultExtensions
{
    public static PagedResult<TResult> Map<T, TResult>(this PagedResult<T> page, Func<T, TResult> map)
        => new(page.Items.Select(map).ToList(), page.Page, page.TotalPages, page.TotalCount);
}
