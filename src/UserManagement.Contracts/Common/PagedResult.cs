namespace UserManagement.Contracts.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int TotalPages, int TotalCount);
