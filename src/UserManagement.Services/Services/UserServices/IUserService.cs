using UserManagement.Contracts.Common;
using UserManagement.Contracts.Users;
using UserManagement.Services.Results;

namespace UserManagement.Services.Services.UserServices;

public interface IUserService
{
    Task<ServiceResult<PagedResult<UserDto>>> GetPageAsync(UserFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserDto>> CreateAsync(UserRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserDto>> UpdateAsync(long id, UserRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
