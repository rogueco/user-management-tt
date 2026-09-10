using UserManagement.Contracts.Common;
using UserManagement.Contracts.Logs;
using UserManagement.Services.Results;

namespace UserManagement.Services.Services.LogServices;

public interface IUserLogService
{
    Task<ServiceResult<PagedResult<LogDto>>> GetPageAsync(LogFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResult<LogDetailsDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
