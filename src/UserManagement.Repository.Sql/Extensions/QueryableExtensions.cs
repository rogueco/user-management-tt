using Microsoft.EntityFrameworkCore;
using UserManagement.Contracts.Common;

namespace UserManagement.Repository.Sql.Extensions;

internal static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1);
        page = Math.Clamp(page, 1, totalPages);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, totalPages, totalCount);
    }
}
